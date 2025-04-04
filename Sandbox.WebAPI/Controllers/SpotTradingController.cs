using Microsoft.AspNetCore.Mvc;
using Sandbox.Application.Services;
using Sandbox.Core.Interfaces;
using Sandbox.Shared.DTOs;

namespace Sandbox.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpotTradingController : ControllerBase
    {
        private readonly IOrderService _orderService;

         public SpotTradingController([FromKeyedServices("Spot")] IOrderService orderService)
         {
             _orderService = orderService;
         }
        
        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDto orderDto)
        {
            var result = await _orderService.PlaceOrderAsync(orderDto);
            return Ok(result);
        }

        [HttpPost("close-order/{orderId}")]
        public async Task<IActionResult> CloseOrder(Guid orderId)
        {
            var result =  await _orderService.CloseOrderAsync(orderId);
            return Ok(result);
        }

        [HttpPost("close-position/{positionId}")]
        public async Task<IActionResult> ClosePosition(Guid positionId)
        {
            var result =  await _orderService.ClosePositionAsync(positionId);
            return Ok(result);
        }

        [HttpPost("set-stop-loss/{positionId}")]
        public async Task<IActionResult> SetStopLoss(Guid positionId, [FromBody] decimal stopLossPrice)
        {
            var result = await _orderService.SetStopLossAsync(positionId, stopLossPrice);
            return Ok(result);
        }

        [HttpPost("set-take-profit/{positionId}")]
        public async Task<IActionResult> SetTakeProfit(Guid positionId, [FromBody] decimal takeProfitPrice)
        {
            var result =  await _orderService.SetTakeProfitAsync(positionId, takeProfitPrice);
            return Ok(result);
        }

        [HttpGet("active-orders/{walletId}")]
        public async Task<IActionResult> GetActiveOrders(Guid walletId)
        {
            var result = await _orderService.GetActiveOrdersAsync(walletId);
            return Ok(result);
        }

        [HttpGet("active-positions/{walletId}")]
        public async Task<IActionResult> GetActivePositions(Guid walletId)
        {
            var result = await _orderService.GetActivePositionsAsync(walletId);
            return Ok(result);
        }
        
        [HttpPost("settrailingstop/{walletId}")]
        public async Task<IActionResult> SetTrailingStop(Guid walletId, [FromBody] string symbol)
        {
            var result = await _orderService.SetTrailingStopAsync(walletId, symbol);
            return Ok(result);
        }
    }
}