using Microsoft.EntityFrameworkCore;
using Sandbox.Core.Entities;

namespace Sandbox.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasKey(a => a.Id);
            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<Position>().HasKey(p => p.Id);
            modelBuilder.Entity<Wallet>().HasKey(w => w.Id);
            
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Wallet)
                .WithOne()
                .HasForeignKey<Wallet>(w => w.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Orders)
                .WithOne(o => o.Wallet)
                .HasForeignKey(o => o.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Positions)
                .WithOne(p => p.Wallet)
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Order>()
                .Property(o => o.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Position>()
                .Property(p => p.OpenedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Position>()
                .Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Symbol);

            modelBuilder.Entity<Position>()
                .HasIndex(p => p.Symbol);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => (e.Entity is Order || e.Entity is Position) &&
                            (e.State == EntityState.Modified || e.State == EntityState.Added));

            foreach (var entry in entries)
            {
                if (entry.Entity is Order order)
                {
                    order.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Position position)
                {
                    position.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }
}
