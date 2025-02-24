using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Sandbox.Application.Interfaces;

namespace Sandbox.Infrastructure.Services
{
    public class BinanceWebSocketService : IPriceService
    {
        private ClientWebSocket _webSocket;
        private readonly Dictionary<string, Action<decimal>> _priceUpdateHandlers;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public BinanceWebSocketService()
        {
            _webSocket = new ClientWebSocket();
            _priceUpdateHandlers = new Dictionary<string, Action<decimal>>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync()
        {
            if (_webSocket.State != WebSocketState.Open)
            {
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws"), CancellationToken.None);
                _ = ReceiveAsync();
            }
        }

        public void Subscribe(string symbol, Action<decimal> onPriceUpdate)
        {
            string lowerSymbol = symbol.ToLower();
            _priceUpdateHandlers[lowerSymbol] = onPriceUpdate;

            var subscribeMessage = new
            {
                method = "SUBSCRIBE",
                @params = new[] { $"{lowerSymbol}@trade" },
                id = 1
            };

            SendMessage(subscribeMessage);
        }

        public void Unsubscribe(string symbol)
        {
            string lowerSymbol = symbol.ToLower();
            if (_priceUpdateHandlers.ContainsKey(lowerSymbol))
            {
                _priceUpdateHandlers.Remove(lowerSymbol);

                var unsubscribeMessage = new
                {
                    method = "UNSUBSCRIBE",
                    @params = new[] { $"{lowerSymbol}@trade" },
                    id = 1
                };

                SendMessage(unsubscribeMessage);
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
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(string message)
        {
            var data = JsonSerializer.Deserialize<BinanceTradeMessage>(message);
            if (data != null && _priceUpdateHandlers.ContainsKey(data.s.ToLower()))
            {
                var price = decimal.Parse(data.p);
                _priceUpdateHandlers[data.s.ToLower()].Invoke(price);
            }
        }

        private async void SendMessage(object message)
        {
            if (_webSocket.State != WebSocketState.Open)
                return;

            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            }
        }
        
        public async Task<decimal> GetPriceAsync(string symbol)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"https://api.binance.com/api/v3/ticker/price?symbol={symbol}");
            var priceData = JsonSerializer.Deserialize<BinancePriceResponse>(response);
            return decimal.Parse(priceData.Price);
        }
        
        private class BinanceTradeMessage
        {
            public string s { get; set; } 
            public string p { get; set; } 
        }

        private class BinancePriceResponse
        {
            public string Symbol { get; set; }
            public string Price { get; set; }
        }
    }
}
