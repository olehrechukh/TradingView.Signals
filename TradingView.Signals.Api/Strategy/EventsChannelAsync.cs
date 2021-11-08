using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingView.Signals.Api.Configurations;

namespace TradingView.Signals.Api.Strategy
{
    public static class EventLoop
    {
        public static async Task RunSimple<T>(
            EventsChannelAsync<T> channel,
            Action<T> eventHandler,
            CancellationToken cancellationToken)
        {
            using (channel)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // var sw = Stopwatch.StartNew();
                    var newEvent = await channel.DequeueOrWaitAsync(cancellationToken);
                    eventHandler(newEvent);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    public class EventsChannelAsync<T> : IReadOnlyCollection<T>, IDisposable
    {
        private readonly ILogger<EventsChannelAsync<T>> logger;
        private const int MaxSize = 20_000;
        private readonly BufferBlock<T> events = new();
        private readonly IOptions<BotConfiguration> configuration;

        public EventsChannelAsync(ILogger<EventsChannelAsync<T>> logger, IOptions<BotConfiguration> configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        private bool isSizeLimitExceeded;
        private volatile bool isDisposed;

        private bool CheckForMaxSize()
        {
            if (!isSizeLimitExceeded && Count > MaxSize)
            {
                isSizeLimitExceeded = true;

                return true;
            }

            return false;
        }

        public void AddEvent(T @event)
        {
            logger.LogInformation("New event, {@value}", @event);

            if (isDisposed || CheckForMaxSize())
            {
                return;
            }

            if (!configuration.Value.EnableTrading)
            {
                logger.LogInformation("Skipping event. Disabled trading, {@value}", @event);
                // return;
            }

            events.Post(@event);
        }

        public async Task<T> DequeueOrWaitAsync(CancellationToken cancellationToken = default)
        {
            CheckForMaxSize();

            var result = await events.ReceiveAsync(cancellationToken);

            // lastRetrievedObject = result;
            // lastRetrieveDateTime = DateTime.UtcNow;

            return result;
        }

        public int Count => events.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return events.TryReceiveAll(out var allEvents)
                ? allEvents.GetEnumerator()
                : Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            isDisposed = true;

            while (events.Count > 0)
            {
                events.TryReceiveAll(out _);
            }
        }
    }
}
