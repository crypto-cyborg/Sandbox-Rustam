using Sandbox.Core.Interfaces;
using Sandbox.Infrastructure.Data;
using Sandbox.Shared.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sandbox.Shared.Results;

namespace Sandbox.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public WalletService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<WalletDto>> GetWalletAsync(Guid walletId)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) throw new Exception("Кошелек не найден");

            return Result<WalletDto>.Success(_mapper.Map<WalletDto>(wallet));
        }

        public async Task<Result<WalletDto>> DepositAsync(Guid walletId, decimal amount)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) throw new ApplicationException("Wallet not found");

            wallet.Balance += amount;
            await _context.SaveChangesAsync();

            return Result<WalletDto>.Success(_mapper.Map<WalletDto>(wallet));
        }

        public async Task<Result<WalletDto>> WithdrawAsync(Guid walletId, decimal amount)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) throw new ApplicationException("Wallet not found");

            if (wallet.Balance < amount) throw new ApplicationException("insufficient balance");

            wallet.Balance -= amount;
            await _context.SaveChangesAsync();

            return Result<WalletDto>.Success(_mapper.Map<WalletDto>(wallet));
        }
    }
}