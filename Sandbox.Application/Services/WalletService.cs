using Sandbox.Core.Interfaces;
using Sandbox.Infrastructure.Data;
using Sandbox.Shared.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sandbox.Core.Entities;
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

        private Task<Wallet?> RetrieveWalletAsync(Guid walletId)
        {
            return _context.Wallets
                .AsNoTracking()
                .Include(w => w.ClosedPositions)
                .Include(wallet => wallet.ClosedOrders)
                .SingleOrDefaultAsync(x => x.Id == walletId);
        }

        public async Task<Result<IEnumerable<OrderDto>>> GetClosedOrdersAsync(Guid walletId)
        {
            var wallet = await RetrieveWalletAsync(walletId);
            
            if (wallet is null) return Result<IEnumerable<OrderDto>>.Failure("Wallet not found");
            
            var dto = _mapper.Map<IEnumerable<OrderDto>>(wallet.ClosedOrders);

            return Result<IEnumerable<OrderDto>>.Success(dto);
        }

        public async Task<Result<IEnumerable<PositionDto>>> GetOpenPositionsAsync(Guid walletId)
        {
            var wallet = await RetrieveWalletAsync(walletId);
            
            if (wallet is null) return Result<IEnumerable<PositionDto>>.Failure("Wallet not found");
            
            var dto = _mapper.Map<IEnumerable<PositionDto>>(wallet.ClosedOrders);
            
            return Result<IEnumerable<PositionDto>>.Success(dto);
        }

        public async Task<Result<PnlDto>> CalculatePnlAsync(Guid walletId, DateTime startDate, DateTime endDate)
        {
            var wallet = await RetrieveWalletAsync(walletId);
            
            if (wallet is null) return Result<PnlDto>.Failure("Wallet not found");

            var pnl = new PnlDto(
                TotalPnl: wallet.CalculatePnL(startDate, endDate),
                Income: wallet.CalculateIncome(startDate, endDate),
                Expenditure: wallet.CalculateExpenditure(startDate, endDate));
            
            return Result<PnlDto>.Success(pnl);
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