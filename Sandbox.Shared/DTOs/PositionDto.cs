namespace Sandbox.Shared.DTOs
{
    public class PositionDto
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public decimal Volume { get; set; }
        public decimal AverageEntryPrice { get; set; } 
        public decimal CurrentPrice { get; set; }
        public string Status { get; set; }     
        public string Direction { get; set; }  
        public decimal Leverage { get; set; }  
        public decimal InitialMargin { get; set; }
        public decimal? StopLossPrice { get; set; }
        public decimal? TakeProfitPrice { get; set; }
        public decimal Pnl { get; set; }
        public decimal PnlPercentage { get; set; }
    }
}