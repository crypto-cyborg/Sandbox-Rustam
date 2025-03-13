using Microsoft.AspNetCore.Mvc;
using Sandbox.Core.Interfaces;
using Sandbox.Shared.DTOs;

namespace Sandbox.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
        {
            var result = await _accountService.CreateAccountAsync(createAccountDto);
            return Ok(result);
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<AccountDto>> GetAccountById(Guid accountId)
        {
            var result = await _accountService.GetAccountByIdAsync(accountId);
            return Ok(result);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            var result = await _accountService.DeleteAccountAsync(id);
            return Ok(result); 
        }

    }
}