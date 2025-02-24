namespace Sandbox.Core.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; }
    public string Symbol { get; set; } 
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public DateTime CreatedAt { get; set; } 
    public DateTime UpdatedAt { get; set; } 
}
