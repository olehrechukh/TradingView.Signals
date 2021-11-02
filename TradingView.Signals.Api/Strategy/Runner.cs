using System;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot.SpotData;
using Microsoft.Extensions.Logging;

namespace TradingView.Signals.Api.Strategy
{
    public interface IExchangeEvent
    {
    }

    public record SignalEvent(string Symbol, OrderSide Side, decimal Price) : IExchangeEvent;

    public record OrderEvent(BinancePlacedOrder Order) : IExchangeEvent;

    public class Runner
    {
        private readonly BinanceClient client;
        private readonly EventsChannelAsync<IExchangeEvent> channel;
        private readonly ILogger<Runner> logger;

        private decimal Size = 0.06m;
        private BinancePlacedOrder order;

        public Runner(
            BinanceClient client,
            EventsChannelAsync<IExchangeEvent> channel,
            ILogger<Runner> logger)
        {
            this.client = client;
            this.channel = channel;
            this.logger = logger;
        }

        public Task Run(CancellationToken cancellationToken) => EventLoop.RunSimple(channel, Handle, cancellationToken);

        private void Handle(IExchangeEvent exchangeEvent)
        {
            switch (exchangeEvent)
            {
                case SignalEvent signalEvent:
                {
                    var quantity = Size;
                    CreateOrderInBackground(signalEvent, quantity);
                    break;
                }
                case OrderEvent orderEvent:
                {
                    order = orderEvent.Order;
                    break;
                }
            }
        }

        private void CreateOrderInBackground(SignalEvent signalEvent, decimal quantity) => RunInBackground(async () =>
        {
            order = null;

            var signalEventSymbol = signalEvent.Symbol == "SOLUSD" ? "SOLUSDT" : signalEvent.Symbol;
            var result = await client.Spot.Order.PlaceOrderAsync(signalEventSymbol, signalEvent.Side, OrderType.Market,
                quantity);

            if (result.Success)
            {
                channel.AddEvent(new OrderEvent(result.Data));
            }
            else
            {
                logger.LogWarning("Can not create order, {@result}", result);
            }
        });

        private static void RunInBackground(Func<Task> action) => Task.Run(action);
    }
}
