namespace Sandbox.Core.Interfaces
{
    public interface IWebSocketService
    {
        Task ConnectAsync();
        Task SubscribeAsync(string symbol, Action<decimal> onPriceUpdate);
        Task UnsubscribeAsync(string symbol);
        Task<decimal> GetPriceAsync(string symbol);

    }
}

