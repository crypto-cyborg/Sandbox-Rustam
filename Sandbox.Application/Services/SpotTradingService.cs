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

        public SpotTradingService(AppDbContext context, BackgroundTrackingService trackingService, IMapper mapper)
        {
            _context = context;
            _trackingService = trackingService;
            _mapper = mapper;
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

        public async Task<Result<OrderDto>> SetStopLossAsync(Guid positionId, decimal stopLossPrice)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) 
                return Result<OrderDto>.Failure("Position not found.");

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
            await _trackingService.SubscribeOrderAsync(stopLossOrder);
            
            return Result<OrderDto>.Success(_mapper.Map<OrderDto>(stopLossOrder));
        }

        public async Task<Result<OrderDto>> SetTakeProfitAsync(Guid positionId, decimal takeProfitPrice)
        {
            var position = await _context.Positions.FindAsync(positionId);
            if (position == null) 
                return Result<OrderDto>.Failure("Position not found.");

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
            return Result<IEnumerable<PositionDto>>.Success(_mapper.Map<IEnumerable<PositionDto>>(positions));
        }
    }
}
