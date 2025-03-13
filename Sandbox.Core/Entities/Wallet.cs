namespace Sandbox.Core.Entities;

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Balance { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<Position> Positions { get; set; } = new List<Position>();
    public List<ClosedOrder> ClosedOrders { get; set; } = new List<ClosedOrder>();
    public List<ClosedPosition> ClosedPositions { get; set; } = new List<ClosedPosition>();
}