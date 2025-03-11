using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Sandbox.Core.Interfaces;
using Sandbox.Shared.DTOs;

namespace Sandbox.WebAPI.Controllers
{
    [ApiController]
    [Route("api/margin-trading")]
    public class MarginTradingController : ControllerBase
    {
        private readonly IOrderService _orderService;


        public MarginTradingController([FromKeyedServices("Margin")] IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDto orderDto)
        {
            var order = await _orderService.PlaceOrderAsync(orderDto);
            return Ok(order);
        }

        [HttpPost("close-order/{orderId}")]
        public async Task<IActionResult> CloseOrder(Guid orderId)
        {
            await _orderService.CloseOrderAsync(orderId);

            return Ok();
        }

        [HttpPost("close-position/{positionId}")]
        public async Task<IActionResult> ClosePosition(Guid positionId)
        {
            await _orderService.ClosePositionAsync(positionId);
            return Ok();
        }

        [HttpPost("set-stop-loss/{positionId}")]
        public async Task<IActionResult> SetStopLoss(Guid positionId, [FromBody] decimal stopLossPrice)
        {
            await _orderService.SetStopLossAsync(positionId, stopLossPrice);
            return Ok();
        }

        [HttpPost("set-take-profit/{positionId}")]
        public async Task<IActionResult> SetTakeProfit(Guid positionId, [FromBody] decimal takeProfitPrice)
        {
            await _orderService.SetTakeProfitAsync(positionId, takeProfitPrice);
            return Ok();
        }

        [HttpGet("active-orders/{walletId}")]
        public async Task<IActionResult> GetActiveOrders(Guid walletId)
        {
            var orders = await _orderService.GetActiveOrdersAsync(walletId);
            return Ok(orders);
        }

        [HttpGet("active-positions/{walletId}")]
        public async Task<IActionResult> GetActivePositions(Guid walletId)
        {
            var positions = await _orderService.GetActivePositionsAsync(walletId);
            return Ok(positions);
        }
    }
}
