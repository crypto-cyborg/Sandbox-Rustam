using Sandbox.Shared.DTOs;

namespace Sandbox.Core.Interfaces;

public interface IWalletService
{
    Task<WalletDto> GetWalletAsync(Guid walletId);
    Task<WalletDto> DepositAsync(Guid walletId, decimal amount);
    Task<WalletDto> WithdrawAsync(Guid walletId, decimal amount);
}