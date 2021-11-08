using System;
using System.Linq;
using Binance.Net.Enums;
using TradingView.Signals.Api.Strategy;
using TradingView.Signals.Api.Strategy.Models;

namespace TradingView.Signals.Api.Controllers
{
    public static class SignalEventParser
    {
        public static bool TryParse(string message, out SignalEvent signalEvent)
        {
            try
            {
                var values = message
                    .Replace(" ", "")
                    .Split(",")
                    .Select(s => s.Split(':'))
                    .Where(x => x.Length == 2)
                    .ToDictionary(strings => strings[0].ToLowerInvariant(), strings => strings[1]);

                var symbol = values["symbol"];
                var side = Enum.Parse<OrderSide>(values["side"], true);
                var price = decimal.Parse(values["price"]);

                signalEvent = new SignalEvent(symbol, side, price);
                return true;
            }
            catch (Exception)
            {
                signalEvent = null;
                return false;
            }
        }
    }
}
