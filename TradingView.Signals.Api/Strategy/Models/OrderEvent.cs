using Binance.Net.Objects.Spot.SpotData;

namespace TradingView.Signals.Api.Strategy.Models
{
    public record OrderEvent(BinancePlacedOrder Order) : IExchangeEvent;
}