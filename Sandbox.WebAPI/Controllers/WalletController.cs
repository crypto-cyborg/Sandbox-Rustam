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
    }
}