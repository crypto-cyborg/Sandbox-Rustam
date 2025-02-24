namespace Sandbox.Core.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
    public List<Position> Positions { get; set; } = new List<Position>();
}