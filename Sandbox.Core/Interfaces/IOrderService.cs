using Sandbox.Core.Entities;
using Sandbox.Core.Enums;
using Sandbox.Shared.DTOs;
using Sandbox.Shared.Results;

namespace Sandbox.Core.Interfaces
{
    public interface IOrderService
    {
        Task<Result<OrderDto>> PlaceOrderAsync(OrderDto orderDto);
        Task <Result<PositionDto>> ClosePositionAsync(Guid positionId);
        Task<Result<OrderDto>> CloseOrderAsync(Guid orderId);
        Task <Result<OrderDto>> SetStopLossAsync(Guid Id, decimal stopLossPrice);
        Task <Result<OrderDto>> SetTakeProfitAsync(Guid positionId, decimal takeProfitPrice);
        Task<Result<IEnumerable<OrderDto>>> GetActiveOrdersAsync(Guid walletId);
        Task<Result<IEnumerable<PositionDto>>> GetActivePositionsAsync(Guid walletId);
        Task<Result<bool>> SetTrailingStopAsync(Guid positionId, string trailingStopDistance);
    }
}

