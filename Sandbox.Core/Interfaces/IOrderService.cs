using Sandbox.Core.Entities;
using Sandbox.Core.Enums;
using Sandbox.Shared.DTOs;

namespace Sandbox.Core.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> PlaceOrderAsync(OrderDto orderDto);
        Task ClosePositionAsync(Guid positionId);
        Task CloseOrderAsync(Guid orderId);
        Task SetStopLossAsync(Guid positionId, decimal stopLossPrice);
        Task SetTakeProfitAsync(Guid positionId, decimal takeProfitPrice);
        Task<IEnumerable<OrderDto>> GetActiveOrdersAsync(Guid walletId);
        Task<IEnumerable<PositionDto>> GetActivePositionsAsync(Guid walletId);
    }
}

