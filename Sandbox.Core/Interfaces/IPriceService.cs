namespace Sandbox.Core.Interfaces
{
    public interface IPriceService
    {
        Task<decimal> GetPriceAsync(string symbol);

        void Subscribe(string symbol, Action<decimal> onPriceUpdate);

        void Unsubscribe(string symbol);
    }
}