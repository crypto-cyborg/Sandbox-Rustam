namespace Sandbox.Core.Entities;

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Balance { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<Position> Positions { get; set; } = new List<Position>();
    public List<ClosedOrder> ClosedOrders { get; set; } = new List<ClosedOrder>();
    public List<ClosedPosition> ClosedPositions { get; set; } = new List<ClosedPosition>();

    public decimal CalculatePnL(DateTime startDate, DateTime endDate)
    {
        return ClosedPositions
            .Where(cp => cp.ClosedAt >= startDate && cp.ClosedAt <= endDate)
            .Sum(cp => cp.CalculatePnL());
    }

    public decimal CalculateIncome(DateTime startDate, DateTime endDate)
    {
        return ClosedPositions
            .Where(cp => cp.ClosedAt >= startDate && cp.ClosedAt <= endDate)
            .Where(cp => cp.CalculatePnL() > 0)
            .Sum(cp => cp.CalculatePnL());
    }
    
    public decimal CalculateExpenditure(DateTime startDate, DateTime endDate)
    {
        return ClosedPositions
            .Where(cp => cp.ClosedAt >= startDate && cp.ClosedAt <= endDate)
            .Where(cp => cp.CalculatePnL() < 0)
            .Sum(cp => cp.CalculatePnL());
    }
}