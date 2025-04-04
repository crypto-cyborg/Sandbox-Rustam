using AutoMapper;
using Sandbox.Core.Interfaces;
using Sandbox.Core.Entities;
using Sandbox.Core.Enums;
using Sandbox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.Infrastructure.Services;
using Sandbox.Shared.DTOs;
using Sandbox.Shared.Results;

namespace Sandbox.Application.Services
{
    public class SpotTradingService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly BackgroundTrackingService _trackingService;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        
        private readonly decimal _trailingStopDistance = 5;

        public SpotTradingService(AppDbContext context, BackgroundTrackingService trackingService, IMapper mapper, IServiceProvider serviceProvider)
        {
            _context = context;
            _trackingService = trackingService;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        public async Task<Result<OrderDto>> PlaceOrderAsync(OrderDto orderDto)
        {
            var order = _mapper.Map<Order>(orderDto);

            var wallet = await _context.Wallets
                .Include(w => w.Orders)
                .Include(w => w.Positions)
                .FirstOrDefaultAsync(w => w.Id == order.WalletId);
            if (wallet == null) 
                return Result<OrderDto>.Failure("Wallet not found");

            if (order.Type == OrderType.Market)
            {
                var price = await _trackingService.GetPriceAsync(order.Symbol);
                if (wallet.Balance < order.Quantity * price)
                    return Result<OrderDto>.Failure("Insufficient balance.");

                order.Status = OrderStatus.Executed;
                order.ExecutedAt = DateTime.UtcNow;
                order.Price = price;
                wallet.Balance -= order.Quantity * price;

                await _trackingService.ExecuteOrderAsync(order, price);
            }
            else if (order.Type == OrderType.Limit)
            {
                if (wallet.Balance < order.Quantity * order.Price)
                    return Result<OrderDto>.Failure("Insufficient balance.");

                wallet.Balance -= order.Quantity * order.Price;
                order.Status = OrderStatus.Open;

                await _trackingService.SubscribeOrderAsync(order);
                _context.Orders.Add(order);
            }

            await _context.SaveChangesAsync();

            if (orderDto.StopLoss.HasValue)
            {
                // var newOrder = _context.Orders.FirstOrDefault(o => o.Id == order.Id);
                SetStopLossAsync(order.Id, (decimal)orderDto.StopLoss);
            }
            if (orderDto.TakeProfit.HasValue)
            {
                // var newOrder = _context.Orders.FirstOrDefault(o => o.Id == order.Id);
                SetTakeProfitAsync(order.Id, (decimal)orderDto.StopLoss);
            }

            return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
        }

        public async Task<Result<OrderDto>> CloseOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Wallet)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return Result<OrderDto>.Failure("Order not found.");
            }
            
            return await _trackingService.CloseOrder(order);
        }
        

        public async Task<Result<PositionDto>> ClosePositionAsync(Guid positionId)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) return Result<PositionDto>.Failure("Position not found.");

            await _trackingService.UnsubscribeSymbolAsync(position.Symbol);
            
            return await _trackingService.ClosePosition(position, false);
        }

        public async Task<Result<OrderDto>> SetStopLossAsync(Guid Id, decimal stopLossPrice)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stopLossOrder = new Order();
            var position = await _context.Positions.FindAsync(Id);
            if (position == null)
            {
                var order = await _context.Orders.FindAsync(Id);
                if (order == null)
                {
                    return Result<OrderDto>.Failure("Order or Position not found.");
                }
                else
                {
                    stopLossOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        WalletId = order.WalletId,
                        Symbol = order.Symbol,
                        Quantity = order.Quantity,
                        Price = stopLossPrice,
                        Type = OrderType.StopLoss,
                        Status = OrderStatus.Open
                    };
                    order.StopLossId = stopLossOrder.Id;
                }

            }
            else
            {
                stopLossOrder = new Order
                {
                    Id = Guid.NewGuid(),
                    WalletId = position.WalletId,
                    Symbol = position.Symbol,
                    Quantity = position.Quantity,
                    Price = stopLossPrice,
                    Type = OrderType.StopLoss,
                    Status = OrderStatus.Open
                };
                position.StopLossId = stopLossOrder.Id;
            }

            context.Orders.Add(stopLossOrder);
            
            await context.SaveChangesAsync();
            await _trackingService.SubscribeOrderAsync(stopLossOrder);
            
            return Result<OrderDto>.Success(_mapper.Map<OrderDto>(stopLossOrder));
        }
        
        

        public async Task<Result<OrderDto>> SetTakeProfitAsync(Guid Id, decimal takeProfitPrice)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var takeProfitOrder = new Order();
            var position = await _context.Positions.FindAsync(Id);
            if (position == null)
            {
                var order = await _context.Orders.FindAsync(Id);
                if (order == null)
                {
                    return Result<OrderDto>.Failure("Order or Position not found.");
                }
                else
                {
                    takeProfitOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        WalletId = order.WalletId,
                        Symbol = order.Symbol,
                        Quantity = order.Quantity,
                        Price = takeProfitPrice,
                        Type = OrderType.StopLoss,
                        Status = OrderStatus.Open
                    };
                    order.StopLossId = takeProfitOrder.Id;
                }

            }
            else
            {
                takeProfitOrder = new Order
                {
                    Id = Guid.NewGuid(),
                    WalletId = position.WalletId,
                    Symbol = position.Symbol,
                    Quantity = position.Quantity,
                    Price = takeProfitPrice,
                    Type = OrderType.StopLoss,
                    Status = OrderStatus.Open
                };
                position.StopLossId = takeProfitOrder.Id;
            }

            context.Orders.Add(takeProfitOrder);
            await context.SaveChangesAsync();
            await _trackingService.SubscribeOrderAsync(takeProfitOrder);
            
            return Result<OrderDto>.Success(_mapper.Map<OrderDto>(takeProfitOrder));
        }

        public async Task<Result<IEnumerable<OrderDto>>> GetActiveOrdersAsync(Guid walletId)
        {
            var orders = await _context.Orders
                .Where(o => o.WalletId == walletId && o.Status == OrderStatus.Open)
                .ToListAsync();
            
            return Result<IEnumerable<OrderDto>>.Success(_mapper.Map<IEnumerable<OrderDto>>(orders));
        }

        public async Task<Result<IEnumerable<PositionDto>>> GetActivePositionsAsync(Guid walletId)
        {
            var positions = await _context.Positions
                .Where(p => p.WalletId == walletId && p.Status == PositionStatus.Open)
                .ToListAsync();
    
            var positionsDto = _mapper.Map<IEnumerable<PositionDto>>(positions);

            foreach (var position in positionsDto)
            {
                position.Pnl = (position.CurrentPrice * position.Quantity) - (position.AverageEntryPrice * position.Quantity);
        
                
                if (position.AverageEntryPrice > 0)
                {
                    position.PnlPercentage = (position.Pnl / (position.AverageEntryPrice * position.Quantity)) * 100;
                }
                else
                {
                    position.PnlPercentage = 0; 
                }
            }

            return Result<IEnumerable<PositionDto>>.Success(positionsDto);
        }

        public async Task<Result<bool>> SetTrailingStopAsync(Guid walletId, string symbol)
        {
            var position = await _context.Positions.SingleOrDefaultAsync(p => p.WalletId == walletId && p.Symbol == symbol);
            if (position == null) throw new ApplicationException("Position not found.");

            position.TrailingStopDistance = _trailingStopDistance;
            position.StopLossPrice = position.Direction == PositionDirection.Long
                ? position.CurrentPrice - (position.CurrentPrice * _trailingStopDistance / 100)
                : position.CurrentPrice + (position.CurrentPrice * _trailingStopDistance / 100);

            await _context.SaveChangesAsync();

            await _trackingService.SubscribePositionAsync(position);
            return Result<bool>.Success(true);
        }
    }
}
