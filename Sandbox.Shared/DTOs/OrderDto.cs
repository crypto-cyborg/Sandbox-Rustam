
namespace Sandbox.Shared.DTOs
{
    public class OrderDto
    {
        public Guid? Id { get; set; }
        public Guid WalletId { get; set; }
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }
        public string OrderType { get; set; } 
        public string? Status { get; set; }    
        public string Direction { get; set; } 
        public decimal? Leverage { get; set; } 
    }

 
}