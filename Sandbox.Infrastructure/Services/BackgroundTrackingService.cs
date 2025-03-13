using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sandbox.Core.Entities;
using Sandbox.Core.Enums;
using Sandbox.Infrastructure.Data;

namespace Sandbox.Infrastructure.Services
{
    public class BackgroundTrackingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ClientWebSocket _webSocket;
        private readonly Dictionary<string, List<Order>> _trackedOrders;
        private readonly Dictionary<string, Position> _trackedPositions;
        private bool _isConnected;

        public BackgroundTrackingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _webSocket = new ClientWebSocket();
            _trackedOrders = new Dictionary<string, List<Order>>();
            _trackedPositions = new Dictionary<string, Position>();
            _isConnected = false;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_trackedOrders.Count > 0 || _trackedPositions.Count > 0)
                {
                    await EnsureWebSocketConnectedAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        public async Task SubscribeOrderAsync(Order order)
        {
            if (!_trackedOrders.ContainsKey(order.Symbol))
            {
                _trackedOrders[order.Symbol] = new List<Order>();
                await SubscribeToSymbolAsync(order.Symbol);
            }

            _trackedOrders[order.Symbol].Add(order);
        }

        public async Task SubscribePositionAsync(Position position)
        {
            if (!_trackedPositions.ContainsKey(position.Symbol))
            {
                _trackedPositions[position.Symbol] = position;
                await SubscribeToSymbolAsync(position.Symbol);
            }
        }

        public async Task UnsubscribeSymbolAsync(string symbol)
        {
            _trackedOrders.Remove(symbol);
            _trackedPositions.Remove(symbol);

            if (_trackedOrders.Count == 0 && _trackedPositions.Count == 0)
            {
                _isConnected = false;
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "No more subscriptions",
                    CancellationToken.None);
            }
        }

        private async Task EnsureWebSocketConnectedAsync()
        {
            if (_isConnected) return;

            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws"), CancellationToken.None);
            _isConnected = true;

            _ = Task.Run(ReceiveAsync);
        }

        private async Task SubscribeToSymbolAsync(string symbol)
        {
            await EnsureWebSocketConnectedAsync();

            var subscribeMessage = new
            {
                method = "SUBSCRIBE",
                @params = new[] { $"{symbol.ToLower()}@ticker" },
                id = 1
            };

            await SendMessageAsync(subscribeMessage);
        }

        private async Task ReceiveAsync()
        {
            var buffer = new byte[1024 * 4];

            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _isConnected = false;
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed",
                        CancellationToken.None);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessMessageAsync(message);
            }
        }

        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                var data = JsonSerializer.Deserialize<BinanceTickerMessage>(message);
                if (data != null &&
                    decimal.TryParse(data.c, NumberStyles.Float, CultureInfo.InvariantCulture, out var price))
                {
                    await HandlePriceUpdateAsync(data.s.ToLower(), price);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        private async Task HandlePriceUpdateAsync(string symbol, decimal currentPrice)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (_trackedOrders.TryGetValue(symbol, out var orders))
            {
                foreach (var order in orders.ToList())
                {
                    if ((order.Direction == PositionDirection.Long && currentPrice <= order.Price) ||
                        (order.Direction == PositionDirection.Short && currentPrice >= order.Price))
                    {
                        await ExecuteOrderAsync(order, currentPrice, context);
                        _trackedOrders[symbol].Remove(order);
                    }
                }

                if (_trackedOrders[symbol].Count == 0)
                {
                    _trackedOrders.Remove(symbol);
                }
            }

            if (_trackedPositions.TryGetValue(symbol, out var position))
            {
                position.CurrentPrice = currentPrice;

                if (position.ShouldLiquidate())
                {
                    await ClosePositionAsync(position, true, context);
                }
                else if (position.ShouldStopLossTrigger() || position.ShouldTakeProfitTrigger())
                {
                    await ClosePositionAsync(position, false, context);
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task ExecuteOrderAsync(Order order, decimal executedPrice, AppDbContext context)
        {
            var wallet = await context.Wallets.Include(w => w.Positions)
                .FirstOrDefaultAsync(w => w.Id == order.WalletId);
            if (wallet == null) return;

            order.Status = OrderStatus.Executed;
            order.ExecutedAt = DateTime.UtcNow;
            order.Price = executedPrice;

            var position = await context.Positions
                .FirstOrDefaultAsync(p =>
                    p.Symbol == order.Symbol && p.WalletId == wallet.Id && p.Status == PositionStatus.Open);

            if (position == null)
            {
                position = new Position
                {
                    WalletId = wallet.Id,
                    Symbol = order.Symbol,
                    Quantity = order.Quantity,
                    AverageEntryPrice = executedPrice,
                    CurrentPrice = executedPrice,
                    Status = PositionStatus.Open,
                    OpenedAt = DateTime.UtcNow,
                    InitialMargin = order.Quantity * executedPrice,
                    Direction = order.Direction
                };
                context.Positions.Add(position);
                await SubscribePositionAsync(position);
            }
            else
            {
                if (position.Direction == order.Direction)
                {
                    var totalQuantity = position.Quantity + order.Quantity;
                    position.AverageEntryPrice =
                        ((position.Quantity * position.AverageEntryPrice) + (order.Quantity * executedPrice)) /
                        totalQuantity;
                    position.Quantity = totalQuantity;
                }
                else
                {
                    var closedSize = Math.Min(position.Quantity, order.Quantity);
                    decimal realizedPnL = (executedPrice - position.AverageEntryPrice) * closedSize *
                                          (position.Direction == PositionDirection.Long ? 1 : -1);
                    wallet.Balance += realizedPnL;

                    if (position.Quantity == closedSize)
                    {
                        position.Status = PositionStatus.Closed;
                        position.ClosedAt = DateTime.UtcNow;
                        context.Positions.Remove(position);
                    }
                    else
                    {
                        position.Quantity -= closedSize;
                    }
                }

                position.CurrentPrice = executedPrice;
            }

            await CloseOrderAsync(order, context);
            await context.SaveChangesAsync();
        }

        private async Task CloseOrderAsync(Order order, AppDbContext context)
        {
            var closedOrder = new ClosedOrder
            {
                Id = order.Id,
                WalletId = order.WalletId,
                Symbol = order.Symbol,
                Quantity = order.Quantity,
                Price = order.Price,
                Type = order.Type,
                Status = OrderStatus.Closed,
                Direction = order.Direction,
                ExecutedAt = order.ExecutedAt,
                CreatedAt = order.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            context.ClosedOrders.Add(closedOrder);
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }

        private async Task ClosePositionAsync(Position position, bool isLiquidation, AppDbContext context)
        {
            position.Status = isLiquidation ? PositionStatus.Liquidated : PositionStatus.Closed;
            position.ClosedAt = DateTime.UtcNow;
            context.Positions.Remove(position);
            await UnsubscribeSymbolAsync(position.Symbol);
            await context.SaveChangesAsync();
        }

        private async Task SendMessageAsync(object message)
        {
            if (_webSocket.State != WebSocketState.Open) return;

            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            using var httpClient = new HttpClient();
            var response =
                await httpClient.GetStringAsync($"https://api.binance.com/api/v3/ticker/price?symbol={symbol}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var priceData = JsonSerializer.Deserialize<BinancePriceResponse>(response, options);

            if (priceData == null || string.IsNullOrEmpty(priceData.Price))
            {
                throw new Exception("Invalid or missing price data from Binance API.");
            }

            return decimal.TryParse(priceData.Price, NumberStyles.Float, CultureInfo.InvariantCulture, out var price)
                ? price
                : throw new Exception("Failed to parse price data.");
        }
    }

    public class BinanceTickerMessage
    {
        public string s { get; set; }
        public string c { get; set; }
    }
    
    public class BinancePriceResponse
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }
    }
}