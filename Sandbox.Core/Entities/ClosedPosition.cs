using Sandbox.Core.Enums;

namespace Sandbox.Core.Entities;

public class ClosedPosition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; }
    public string Symbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageEntryPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public PositionStatus Status { get; set; }
    public PositionDirection Direction { get; set; }
    public decimal Leverage { get; set; } = 1;
    public decimal InitialMargin { get; set; }
    public decimal MaintenanceMarginRate { get; set; } = 0.25m;

    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public decimal MaintenanceMargin => InitialMargin * MaintenanceMarginRate;

    public decimal CalculatePnL()
    {
        return Direction == PositionDirection.Long
            ? (CurrentPrice - AverageEntryPrice) * Quantity * Leverage
            : (AverageEntryPrice - CurrentPrice) * Quantity * Leverage;
    }
    
    public decimal CalculatePnLPercentage()
    {
        return InitialMargin > 0 ? (CalculatePnL() / InitialMargin) * 100 : 0;
    }
}