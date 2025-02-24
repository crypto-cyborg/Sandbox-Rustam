namespace Sandbox.Core.Entities;

public class Position
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageEntryPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public PositionStatus Status { get; set; } = PositionStatus.Open;
    public decimal? StopLossPrice { get; set; }
    public decimal? TakeProfitPrice { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
