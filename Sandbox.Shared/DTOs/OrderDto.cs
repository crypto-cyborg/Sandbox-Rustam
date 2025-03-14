
namespace Sandbox.Shared.DTOs
{
    public class OrderDto
    {
        public Guid? Id { get; set; }
        public Guid WalletId { get; set; }
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal Volume { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
        public string OrderType { get; set; } 
        public string? Status { get; set; }    
        public string Direction { get; set; } 
        public decimal? Leverage { get; set; } 
    }

 
}