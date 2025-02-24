using Microsoft.AspNetCore.Mvc;
using Sandbox.Application.Interfaces;
using Sandbox.Core.Entities;

namespace Sandbox.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotTradingController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public SpotTradingController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            var placedOrder = await _orderService.PlaceOrderAsync(order);
            return Ok(placedOrder);
        }

        [HttpPost("close-order/{orderId}")]
        public async Task<IActionResult> CloseOrder(Guid orderId)
        {
            await _orderService.CloseOrderAsync(orderId);
            return Ok();
        }

        [HttpPost("set-stop-loss/{positionId}")]
        public async Task<IActionResult> SetStopLoss(Guid positionId, [FromBody] decimal stopLossPrice)
        {
            // Реализовать установку стоп-лосса
            return Ok("Стоп-лосс установлен.");
        }

        [HttpPost("set-take-profit/{positionId}")]
        public async Task<IActionResult> SetTakeProfit(Guid positionId, [FromBody] decimal takeProfitPrice)
        {
            // Реализовать установку тейк-профита
            return Ok("Тейк-профит установлен.");
        }
    }
}