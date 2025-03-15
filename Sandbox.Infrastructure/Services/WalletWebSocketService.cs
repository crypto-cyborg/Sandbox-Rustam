using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.Infrastructure.Data;
using Sandbox.Shared.DTOs;

namespace Sandbox.Infrastructure.Services
{
    public class WalletWebSocketService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();
        private readonly IMapper _mapper;

        public WalletWebSocketService(IServiceProvider serviceProvider, IMapper mapper)
        {
            _serviceProvider = serviceProvider;
            _mapper = mapper;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, Guid walletId)
        {
            _sockets[walletId] = webSocket;

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var wallet = await dbContext.Wallets
                        .Include(w => w.Orders)
                        .Include(w => w.Positions)
                        .FirstOrDefaultAsync(w => w.Id == walletId);

                    if (wallet != null)
                    {
                        var dto = _mapper.Map<WalletDto>(wallet);
                        var json = JsonSerializer.Serialize(dto);
                        var buffer = Encoding.UTF8.GetBytes(json);
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket Error: {ex.Message}");
            }
            finally
            {
                _sockets.Remove(walletId, out _);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
            }
        }

        public async Task BroadcastUpdate(Guid walletId)
        {
            if (_sockets.TryGetValue(walletId, out var socket) && socket.State == WebSocketState.Open)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
                var wallet = await dbContext.Wallets
                    .Include(w => w.Orders)
                    .Include(w => w.Positions)
                    .FirstOrDefaultAsync(w => w.Id == walletId);
        
                if (wallet != null)
                {
                    var response = new
                    {
                        Balance = wallet.Balance,
                        Orders = wallet.Orders.Select(o => new { o.Id, o.Symbol, o.Status, o.Quantity, o.Price }),
                        Positions = wallet.Positions.Select(p => new { p.Id, p.Symbol, p.Quantity, p.AverageEntryPrice, p.CurrentPrice })
                    };
        
                    var json = JsonSerializer.Serialize(response);
                    var buffer = Encoding.UTF8.GetBytes(json);
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}


