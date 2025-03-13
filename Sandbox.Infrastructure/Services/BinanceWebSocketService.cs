using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandbox.Core.Interfaces;

namespace Sandbox.Infrastructure.Services
{
    public class BinanceWebSocketService : IWebSocketService
    {
        private ClientWebSocket _webSocket;
        private readonly Dictionary<string, Action<decimal>> _priceUpdateHandlers;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isConnected;

        public BinanceWebSocketService()
        {
            _webSocket = new ClientWebSocket();
            _priceUpdateHandlers = new Dictionary<string, Action<decimal>>();
            _cancellationTokenSource = new CancellationTokenSource();
            _isConnected = false;
        }

        public async Task ConnectAsync()
        {
            if (_isConnected) return;

            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws"), CancellationToken.None);
            _isConnected = true;

            _ = Task.Run(ReceiveAsync);
        }

        public async Task SubscribeAsync(string symbol, Action<decimal> onPriceUpdate)
        {
            await ConnectAsync();

            string lowerSymbol = symbol.ToLower();
            if (!_priceUpdateHandlers.ContainsKey(lowerSymbol))
                _priceUpdateHandlers[lowerSymbol] = onPriceUpdate;
            else
                _priceUpdateHandlers[lowerSymbol] += onPriceUpdate;

            var subscribeMessage = new
            {
                method = "SUBSCRIBE",
                @params = new[] { $"{lowerSymbol}@ticker" },  
                id = 1
            };

            await SendMessageAsync(subscribeMessage);
        }

        public async Task UnsubscribeAsync(string symbol)
        {
            string lowerSymbol = symbol.ToLower();
            if (_priceUpdateHandlers.ContainsKey(lowerSymbol))
            {
                _priceUpdateHandlers.Remove(lowerSymbol);

                if (_priceUpdateHandlers.Count == 0)
                {
                    _isConnected = false;
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "No more subscribers", CancellationToken.None);
                }

                var unsubscribeMessage = new
                {
                    method = "UNSUBSCRIBE",
                    @params = new[] { $"{lowerSymbol}@ticker" }, 
                    id = 1
                };

                await SendMessageAsync(unsubscribeMessage);
            }
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
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var data = JsonSerializer.Deserialize<BinanceTickerMessage>(message);
                if (data != null && _priceUpdateHandlers.TryGetValue(data.s.ToLower(), out var handler))
                {
                    if (decimal.TryParse(data.c, NumberStyles.Float, CultureInfo.InvariantCulture, out var price))
                    {
                        handler.Invoke(price);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        private async Task SendMessageAsync(object message)
        {
            if (_webSocket.State != WebSocketState.Open) return;

            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"https://api.binance.com/api/v3/ticker/price?symbol={symbol}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
    
            var priceData = JsonSerializer.Deserialize<BinancePriceResponse>(response, options);

            if (priceData == null || string.IsNullOrEmpty(priceData.Price))
            {
                throw new Exception("Invalid or missing price data from Binance API.");
            }

            return decimal.TryParse(priceData.Price, System.Globalization.NumberStyles.Float, 
                System.Globalization.CultureInfo.InvariantCulture, out var price) 
                ? price 
                : throw new Exception("Failed to parse price data.");
        }

        public class BinancePriceResponse
        {
            [JsonPropertyName("symbol")]
            public string Symbol { get; set; }

            [JsonPropertyName("price")]
            public string Price { get; set; }
        }


        private class BinanceTickerMessage
        {
            public string s { get; set; }  // Symbol
            public string c { get; set; }  // Current price 
        }


    }
}
