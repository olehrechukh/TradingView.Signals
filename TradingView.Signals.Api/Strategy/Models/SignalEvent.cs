using Binance.Net.Enums;

namespace TradingView.Signals.Api.Strategy.Models
{
    public record SignalEvent(string Symbol, OrderSide Side, decimal Price) : IExchangeEvent;
}