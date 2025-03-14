using Microsoft.AspNetCore.Mvc;
using Sandbox.Core.Interfaces;
using Sandbox.Shared.DTOs;

namespace Sandbox.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("{walletId}")]
        public async Task<ActionResult<WalletDto>> GetWallet(Guid walletId)
        {
            var result = await _walletService.GetWalletAsync(walletId);
            return Ok(result);
        }
        
        [HttpPost("{walletId}/deposit")]
        public async Task<ActionResult<WalletDto>> Deposit(Guid walletId, [FromBody] WalletTransactionDto transaction)
        {
            var result = await _walletService.DepositAsync(walletId, transaction.Amount);
            return Ok(result);
        }
        
        [HttpPost("{walletId}/withdraw")]
        public async Task<ActionResult<WalletDto>> Withdraw(Guid walletId, [FromBody] WalletTransactionDto transaction)
        {
            var result = await _walletService.WithdrawAsync(walletId, transaction.Amount);
            return Ok(result);
        }

        [HttpGet("{walletId}/orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetClosedOrders(Guid walletId)
        {
            var result = await _walletService.GetClosedOrdersAsync(walletId);
            return Ok(result);
        }

        [HttpGet("{walletId}/positions")]
        public async Task<ActionResult<PositionDto>> GetClosedPositions(Guid walletId)
        {
            var result = await _walletService.GetOpenPositionsAsync(walletId);
            return Ok(result);
        }

        public record GetPnlRequest(DateTime StartDate, DateTime EndDate);
        [HttpGet("{walletId}/pnl")]
        public async Task<ActionResult<decimal>> GetPnl(Guid walletId, [FromBody] GetPnlRequest request)
        {
            var result = await _walletService.CalculatePnlAsync(walletId, request.StartDate, request.EndDate);
            return Ok(result);
        }
    }
}