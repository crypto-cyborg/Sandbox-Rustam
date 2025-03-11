using AutoMapper;
using Sandbox.Core.Interfaces;
using Sandbox.Core.Entities;
using Sandbox.Core.Enums;
using Sandbox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.Infrastructure.Services;
using Sandbox.Shared.DTOs;

namespace Sandbox.Application.Services
{
    public class SpotTradingService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly BinanceWebSocketService _webSocketService;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;

        public SpotTradingService(AppDbContext context, BinanceWebSocketService webSocketService, IMapper mapper,
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _webSocketService = webSocketService;
            _mapper = mapper;
            _scopeFactory = scopeFactory;
        }

        public async Task<OrderDto> PlaceOrderAsync(OrderDto orderDto)
        {
            var wallet = await _context.Wallets
                .Include(w => w.Orders)
                .Include(w => w.Positions)
                .FirstOrDefaultAsync(w => w.Id == orderDto.WalletId);

            if (wallet == null) throw new ApplicationException("Wallet not found.");

            var order = _mapper.Map<Order>(orderDto);
            if (wallet.Balance < order.Quantity * order.Price)
                throw new ApplicationException("Insufficient balance.");

            wallet.Balance -= order.Quantity * order.Price;
            order.Status = OrderStatus.Open;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _webSocketService.SubscribeAsync(order.Symbol, async (currentPrice) =>
            {
                await TrackPosition(order, currentPrice);
            });

            return _mapper.Map<OrderDto>(order);
        }


        private async Task TrackPosition(Order order, decimal currentPrice)
        {
            var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var position = await _context.Positions
                .FirstOrDefaultAsync(p =>
                    p.Symbol == order.Symbol && p.WalletId == order.WalletId && p.Status == PositionStatus.Open);

            if (position == null)
            {
                if (order.Type == OrderType.Market || (order.Type == OrderType.Limit && order.Price <= currentPrice))
                {
                    await ExecuteOrderAsync(order, currentPrice);
                }

                return;
            }

            position.CurrentPrice = currentPrice;

            if (position.ShouldLiquidate())
            {
                await ClosePosition(position, position.ShouldLiquidate());
                _webSocketService.UnsubscribeAsync(order.Symbol);
            }
            else if (position.ShouldStopLossTrigger() || position.ShouldTakeProfitTrigger())
            {
                await ClosePosition(position, false);
                _webSocketService.UnsubscribeAsync(order.Symbol);
            }
            else
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task CloseOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.Include(o => o.Wallet).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) throw new ApplicationException("Order not found.");

            if (order.Type == OrderType.StopLoss || order.Type == OrderType.TakeProfit)
            {
                var position = await _context.Positions
                    .FirstOrDefaultAsync(p => (p.StopLossOrderId == order.Id || p.TakeProfitOrderId == order.Id));

                if (position != null)
                {
                    if (position.StopLossOrderId == order.Id)
                    {
                        position.StopLossOrderId = null;
                        position.StopLossOrder = null;
                    }

                    if (position.TakeProfitOrderId == order.Id)
                    {
                        position.TakeProfitOrderId = null;
                        position.TakeProfitOrder = null;
                    }
                }
            }

            order.Status = OrderStatus.Closed;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ExecuteOrderAsync(Order order, decimal executedPrice)
        {
            var wallet = await _context.Wallets.Include(w => w.Positions)
                .FirstOrDefaultAsync(w => w.Id == order.WalletId);
            if (wallet == null) return;

            order.Status = OrderStatus.Executed;
            order.ExecutedAt = DateTime.UtcNow;
            order.Price = executedPrice;

            var position = await _context.Positions
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
                    OpenedAt = DateTime.UtcNow
                };
                _context.Positions.Add(position);
            }
            else
            {
                var totalQuantity = position.Quantity + order.Quantity;
                if (position.Direction == order.Direction)
                {
                    position.AverageEntryPrice =
                        ((position.Quantity * position.AverageEntryPrice) + (order.Quantity * executedPrice)) /
                        totalQuantity;
                }
                else
                {
                    var closedSize = Math.Min(position.Quantity, order.Quantity);
                    decimal realizedPnL = (executedPrice - position.AverageEntryPrice) * closedSize *
                                          (position.Direction == PositionDirection.Long ? 1 : -1);
                    wallet.Balance += realizedPnL;

                    if (position.Quantity == order.Quantity)
                    {
                        position.Status = PositionStatus.Closed;
                        position.ClosedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        position.Quantity -= order.Quantity;
                    }
                }

                position.Quantity = totalQuantity;
                position.CurrentPrice = executedPrice;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ClosePositionAsync(Guid positionId)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) throw new ApplicationException("Position not found.");

            await ClosePosition(position, false);
        }

        private async Task ClosePosition(Position position, bool isLiquidation)
        {
            var wallet = await _context.Wallets.FindAsync(position.WalletId);
            if (wallet == null) return;

            decimal pnl;
            if (position.Direction == PositionDirection.Long)
            {
                pnl = (position.CurrentPrice - position.AverageEntryPrice) * position.Quantity * position.Leverage;
            }
            else
            {
                pnl = (position.AverageEntryPrice - position.CurrentPrice) * position.Quantity * position.Leverage;
            }

            if (isLiquidation)
                pnl = Math.Min(pnl, 0);

            wallet.Balance += pnl;
            position.Status = isLiquidation ? PositionStatus.Liquidated : PositionStatus.Closed;
            position.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task SetStopLossAsync(Guid positionId, decimal stopLossPrice)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) throw new ApplicationException("Position not found.");

            var stopLossOrder = new Order
            {
                Id = Guid.NewGuid(),
                WalletId = position.WalletId,
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                Price = stopLossPrice,
                Type = OrderType.StopLoss,
                Status = OrderStatus.Open
            };

            _context.Orders.Add(stopLossOrder);
            position.StopLossOrderId = stopLossOrder.Id;
            await _context.SaveChangesAsync();
        }

        public async Task SetTakeProfitAsync(Guid positionId, decimal takeProfitPrice)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) throw new ApplicationException("Position not found.");

            var takeProfitOrder = new Order
            {
                Id = Guid.NewGuid(),
                WalletId = position.WalletId,
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                Price = takeProfitPrice,
                Type = OrderType.TakeProfit,
                Status = OrderStatus.Open
            };

            _context.Orders.Add(takeProfitOrder);
            position.TakeProfitOrderId = takeProfitOrder.Id;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderDto>> GetActiveOrdersAsync(Guid walletId)
        {
            var orders = await _context.Orders
                .Where(o => o.WalletId == walletId && o.Status == OrderStatus.Open)
                .ToListAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<PositionDto>> GetActivePositionsAsync(Guid walletId)
        {
            var positions = await _context.Positions
                .Where(p => p.WalletId == walletId && p.Status == PositionStatus.Open)
                .ToListAsync();
            return _mapper.Map<IEnumerable<PositionDto>>(positions);
        }
    }
}