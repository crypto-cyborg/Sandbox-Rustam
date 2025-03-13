using Sandbox.Shared.DTOs;
using Sandbox.Shared.Results;

namespace Sandbox.Core.Interfaces;

public interface IWalletService
{
    Task<Result<WalletDto>> GetWalletAsync(Guid walletId);
    Task<Result<WalletDto>> DepositAsync(Guid walletId, decimal amount);
    Task<Result<WalletDto>> WithdrawAsync(Guid walletId, decimal amount);
}