using System;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot.SpotData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingView.Signals.Api.Configurations;
using TradingView.Signals.Api.Strategy.Models;

namespace TradingView.Signals.Api.Strategy
{
    public class Runner
    {
        private readonly BinanceClient client;
        private readonly EventsChannelAsync<IExchangeEvent> channel;
        private readonly ILogger<Runner> logger;

        private decimal Size = 0.06m;
        private BinancePlacedOrder order;
        private readonly IOptions<BotConfiguration> configuration;

        public Runner(
            BinanceClient client,
            EventsChannelAsync<IExchangeEvent> channel,
            IOptions<BotConfiguration> configuration,
            ILogger<Runner> logger)
        {
            this.client = client;
            this.channel = channel;
            this.configuration = configuration;
            this.logger = logger;
        }

        public Task Run(CancellationToken cancellationToken) => EventLoop.RunSimple(channel, Handle, cancellationToken);

        private void Handle(IExchangeEvent exchangeEvent)
        {
            if (!configuration.Value.EnableTrading)
            {
                logger.LogInformation("Skipping event. Disabled trading, {@value}", exchangeEvent);
                return;
            }

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

            var signalEventSymbol = signalEvent.Symbol == "ADAUSD" ? "ADAUSDT" : signalEvent.Symbol;
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
