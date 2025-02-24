using Sandbox.Application.Interfaces;
using Sandbox.Core.Entities;
using Sandbox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Sandbox.Infrastructure.Services
{
    public class SpotTradingService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly BinanceWebSocketService _webSocketService;

        public SpotTradingService(AppDbContext context, BinanceWebSocketService webSocketService)
        {
            _context = context;
            _webSocketService = webSocketService;
        }

        public async Task<Order> PlaceOrderAsync(Order order)
        {
            var wallet = await _context.Wallets.Include(w => w.Orders).Include(w => w.Positions)
                .FirstOrDefaultAsync(w => w.Id == order.WalletId);

            if (wallet == null || wallet.Balance < order.Quantity * order.Price)
                throw new Exception("Недостаточно средств или кошелек не найден.");

            wallet.Balance -= order.Quantity * order.Price;
            order.Status = OrderStatus.Open;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _webSocketService.Subscribe(order.Symbol, async (currentPrice) =>
            {
                await TrackOrder(order, currentPrice);
            });

            return order;
        }

        private async Task TrackOrder(Order order, decimal currentPrice)
        {
            var position = await _context.Positions.FirstOrDefaultAsync(
                p => p.Symbol == order.Symbol && p.WalletId == order.WalletId && p.Status == PositionStatus.Open);

            if (position != null)
            {
                position.CurrentPrice = currentPrice;

                // Проверяем Stop Loss
                if (position.StopLossPrice.HasValue && currentPrice <= position.StopLossPrice.Value)
                {
                    await ClosePosition(position, true);
                }

                // Проверяем Take Profit
                if (position.TakeProfitPrice.HasValue && currentPrice >= position.TakeProfitPrice.Value)
                {
                    await ClosePosition(position, false);
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var newPosition = new Position
                {
                    WalletId = order.WalletId,
                    Symbol = order.Symbol,
                    Quantity = order.Quantity,
                    AverageEntryPrice = order.Price,
                    CurrentPrice = currentPrice,
                    Status = PositionStatus.Open,
                    OpenedAt = DateTime.UtcNow
                };

                _context.Positions.Add(newPosition);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ClosePosition(Position position, bool isStopLoss)
        {
            var wallet = await _context.Wallets.FindAsync(position.WalletId);

            if (wallet != null)
            {
                decimal pnl = (position.CurrentPrice - position.AverageEntryPrice) * position.Quantity;

                if (isStopLoss)
                    pnl = Math.Min(pnl, 0);

                wallet.Balance += pnl;

                position.Status = PositionStatus.Closed;
                position.ClosedAt = DateTime.UtcNow;
                position.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        public async Task CloseOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) throw new Exception("Ордер не найден.");

            var wallet = await _context.Wallets.Include(w => w.Positions).FirstOrDefaultAsync(w => w.Id == order.WalletId);

            var position = wallet.Positions.FirstOrDefault(p => p.Symbol == order.Symbol && p.Status == PositionStatus.Open);
            if (position != null)
            {
                var profitLoss = (position.CurrentPrice - position.AverageEntryPrice) * position.Quantity;
                wallet.Balance += profitLoss;
                position.Status = PositionStatus.Closed;
                position.ClosedAt = DateTime.UtcNow;

                order.Status = OrderStatus.Closed;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetStopLossAsync(Guid positionId, decimal stopLossPrice)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) throw new Exception("Позиция не найдена.");

            position.StopLossPrice = stopLossPrice;
            position.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task SetTakeProfitAsync(Guid positionId, decimal takeProfitPrice)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) throw new Exception("Позиция не найдена.");

            position.TakeProfitPrice = takeProfitPrice;
            position.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetActiveOrdersAsync(Guid walletId)
        {
            return await _context.Orders
                .Where(o => o.WalletId == walletId && o.Status == OrderStatus.Open)
                .ToListAsync();
        }

        public async Task<IEnumerable<Position>> GetActivePositionsAsync(Guid walletId)
        {
            return await _context.Positions
                .Where(p => p.WalletId == walletId && p.Status == PositionStatus.Open)
                .ToListAsync();
        }
    }
}
