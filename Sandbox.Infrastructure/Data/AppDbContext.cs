using Microsoft.EntityFrameworkCore;
using Sandbox.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.Infrastructure.Services;

namespace Sandbox.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<ClosedOrder> ClosedOrders { get; set; }
        public DbSet<ClosedPosition> ClosedPositions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        
        private readonly IServiceProvider _serviceProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider) 
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasKey(a => a.Id);
            modelBuilder.Entity<Wallet>().HasKey(w => w.Id);
            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<Position>().HasKey(p => p.Id);
            modelBuilder.Entity<ClosedOrder>().HasKey(w => w.Id);
            modelBuilder.Entity<ClosedPosition>().HasKey(w => w.Id);
            
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Wallet)
                .WithOne()
                .HasForeignKey<Wallet>(w => w.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Wallet)
                .WithMany(w => w.Orders)
                .HasForeignKey(o => o.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.Wallet)
                .WithMany(w => w.Positions)
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ClosedOrder>()
                .HasOne(o => o.Wallet)
                .WithMany(w => w.ClosedOrders)
                .HasForeignKey(o => o.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClosedPosition>()
                .HasOne(p => p.Wallet)
                .WithMany(w => w.ClosedPositions)
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.StopLossOrder)
                .WithOne()
                .HasForeignKey<Position>(p => p.StopLossId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.TakeProfitOrder)
                .WithOne()
                .HasForeignKey<Position>(p => p.TakeProfitId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Order>().Property(o => o.Quantity).HasPrecision(18, 8);
            modelBuilder.Entity<Order>().Property(o => o.Price).HasPrecision(18, 8);
            modelBuilder.Entity<Order>().Property(o => o.Leverage).HasPrecision(18, 2);
            modelBuilder.Entity<Position>().Property(p => p.Quantity).HasPrecision(18, 8);
            modelBuilder.Entity<Position>().Property(p => p.AverageEntryPrice).HasPrecision(18, 8);
            modelBuilder.Entity<Position>().Property(p => p.CurrentPrice).HasPrecision(18, 8);
            modelBuilder.Entity<Position>().Property(p => p.InitialMargin).HasPrecision(18, 8);
            modelBuilder.Entity<Position>().Property(p => p.Leverage).HasPrecision(18, 2);
            modelBuilder.Entity<Position>().Property(p => p.MaintenanceMarginRate).HasPrecision(18, 4);
            modelBuilder.Entity<Wallet>().Property(w => w.Balance).HasPrecision(18, 8);
        }
        

    }
}
