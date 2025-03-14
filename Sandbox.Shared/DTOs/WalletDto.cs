namespace Sandbox.Shared.DTOs
{
    public class WalletDto
    {
        public Guid Id { get; set; }
        public decimal Balance { get; set; }
        public decimal Pnl { get; set; }
        public decimal PnlPercentage { get; set; }
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
        public List<PositionDto> Positions { get; set; } = new List<PositionDto>();
    }

    public class WalletTransactionDto
    {
        public decimal Amount { get; set; }
    }
}