using System.Globalization;
using System.Threading;
using Binance.Net.Enums;
using FluentAssertions;
using TradingView.Signals.Api.Controllers;
using Xunit;

namespace TradingView.Signals.UnitTests
{
    public class ParserTests
    {
        private const string Input =
            "exchange:BINANCE, symbol:SOLUSD, price:204.386075766, volume:199.06070501, side:Buy";

        public ParserTests()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [Fact]
        public void ShouldParseSignalEvent()
        {
            var tryParse = SignalEventParser.TryParse(Input, out var signalEvent);

            tryParse.Should().BeTrue();
            signalEvent.Symbol.Should().Be("SOLUSD");
            signalEvent.Price.Should().Be(204.386075766m);
            signalEvent.Side.Should().Be(OrderSide.Buy);
        }
    }
}
