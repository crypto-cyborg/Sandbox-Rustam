using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Sandbox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Sandbox.Core.Enums;

namespace Sandbox.WebAPI.Services
{
    public class PositionWebSocketService
    {
        private readonly Dictionary<Guid, WebSocket> _connectedClients = new();
        private readonly AppDbContext _context;

        public PositionWebSocketService(AppDbContext context)
        {
            _context = context;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, Guid walletId)
        {
            _connectedClients[walletId] = webSocket;

            while (webSocket.State == WebSocketState.Open)
            {
                var positions = await _context.Positions
                    .Where(p => p.WalletId == walletId && p.Status == PositionStatus.Open)
                    .ToListAsync();

                var json = JsonSerializer.Serialize(positions);
                var buffer = Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(buffer);

                await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);

                await Task.Delay(1000);
            }

            _connectedClients.Remove(walletId);
        }
    }
}