using Sandbox.Core.Entities;

namespace Sandbox.Application.Interfaces
{
    public interface IOrderService
    {
        // Создание нового ордера
        Task<Order> PlaceOrderAsync(Order order);

        // Закрытие ордера по идентификатору
        Task CloseOrderAsync(Guid orderId);

        // Установка стоп-лосса для позиции
        Task SetStopLossAsync(Guid positionId, decimal stopLossPrice);

        // Установка тейк-профита для позиции
        Task SetTakeProfitAsync(Guid positionId, decimal takeProfitPrice);

        // Получение активных ордеров для конкретного кошелька
        Task<IEnumerable<Order>> GetActiveOrdersAsync(Guid walletId);

        // Получение активных позиций для конкретного кошелька
        Task<IEnumerable<Position>> GetActivePositionsAsync(Guid walletId);
    }
}