using Sandbox.Core.Enums;

namespace Sandbox.Core.Entities;

public class Position
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
    public Guid? StopLossId { get; set; }
    public Order? StopLossOrder { get; set; }
    public Guid? TakeProfitId { get; set; }
    public Order? TakeProfitOrder { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal? TrailingStopDistance { get; set; } 
    public decimal? StopLossPrice { get; set; } 
    
    public bool ShouldUpdateTrailingStop(decimal newPrice)
    {
        if (!TrailingStopDistance.HasValue || !StopLossPrice.HasValue)
            return false;

        if (Direction == PositionDirection.Long)
        {
            decimal newStopLoss = newPrice - (newPrice * TrailingStopDistance.Value / 100);
            return newStopLoss > StopLossPrice.Value;
        }
        else
        {
            decimal newStopLoss = newPrice + (newPrice * TrailingStopDistance.Value / 100);
            return newStopLoss < StopLossPrice.Value;
        }
    }
    

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

    public bool ShouldLiquidate()
    {
        var unrealizedPnl = CalculatePnL();
        return (InitialMargin + unrealizedPnl) < MaintenanceMargin;
    }

    public bool ShouldStopLossTrigger()
    {
        return StopLossOrder != null && StopLossOrder.Price > 0 && (
            (Direction == PositionDirection.Long && CurrentPrice <= StopLossOrder.Price) ||
            (Direction == PositionDirection.Short && CurrentPrice >= StopLossOrder.Price)
        );
    }

    public bool ShouldTakeProfitTrigger()
    {
        return TakeProfitOrder != null && TakeProfitOrder.Price > 0 && (
            (Direction == PositionDirection.Long && CurrentPrice >= TakeProfitOrder.Price) ||
            (Direction == PositionDirection.Short && CurrentPrice <= TakeProfitOrder.Price)
        );
    }
}