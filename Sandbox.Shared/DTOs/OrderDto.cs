using Newtonsoft.Json;

namespace Sandbox.Shared.DTOs
{
    public class OrderDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }
        public Guid WalletId { get; set; }
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Price { get; set; }
        public string OrderType { get; set; } 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        
        public string? Status { get; set; }    
        public string Direction { get; set; } 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Leverage { get; set; } 
    }

    // public class CreateOrderDto
    // {
    //     public Guid WalletId { get; set; }
    //     public string Symbol { get; set; }
    //     public decimal Quantity { get; set; }
    //     public string OrderType { get; set; } 
    //     public string Direction { get; set; } 
    //     public decimal? Leverage { get; set; } 
    //     public decimal? Price { get; set; }
    // }
}