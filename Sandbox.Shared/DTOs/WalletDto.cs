namespace Sandbox.Shared.DTOs
{
    public class WalletDto
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
    }

    public class WalletTransactionDto
    {
        public decimal Amount { get; set; }
    }
}