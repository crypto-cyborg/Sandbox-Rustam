using Sandbox.Shared.DTOs;
using Sandbox.Shared.Results;

namespace Sandbox.Core.Interfaces;

public interface IAccountService
{
      Task<Result<AccountDto>> CreateAccountAsync(CreateAccountDto createAccountDto);
      Task<Result<AccountDto>> GetAccountByIdAsync(Guid accountId);
      Task<Result<AccountDto>> DeleteAccountAsync(Guid id);
}