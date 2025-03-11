using Sandbox.Shared.DTOs;

namespace Sandbox.Core.Interfaces;

public interface IAccountService
{
      Task<AccountDto> CreateAccountAsync(CreateAccountDto createAccountDto);
      Task<AccountDto> GetAccountByIdAsync(Guid accountId);
      Task DeleteAccountAsync(Guid id);
}