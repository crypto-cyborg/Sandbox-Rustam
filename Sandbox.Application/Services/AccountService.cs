using Sandbox.Core.Entities;
using Sandbox.Core.Interfaces;
using Sandbox.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Sandbox.Shared.DTOs;
using AutoMapper;
using Sandbox.Shared.Results;

namespace Sandbox.Application.Services;

    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AccountService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<AccountDto>> CreateAccountAsync(CreateAccountDto createAccountDto)
        {
            if (await _context.Accounts.AnyAsync(a => a.Email == createAccountDto.Email))
            {
                return Result<AccountDto>.Failure("Email already exists");
            }

            var account = new Account
            {
                Email = createAccountDto.Email,
                Wallet = new Wallet() 
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
        }

        public async Task<Result<AccountDto>> DeleteAccountAsync(Guid id)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
            {
                return Result<AccountDto>.Failure("Account not found");
            }

            _context.Accounts.Remove(account); 

            await _context.SaveChangesAsync();
            
            return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
        }


        public async Task<Result<AccountDto>> GetAccountByIdAsync(Guid accountId)
        {
            var account = await _context.Accounts.Include(a => a.Wallet).FirstOrDefaultAsync(a => a.Id == accountId);
            if (account == null)
            {
                return Result<AccountDto>.Failure("Account not found");
            }

            return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
        }
    }
