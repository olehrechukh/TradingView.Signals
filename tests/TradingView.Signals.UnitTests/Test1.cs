using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net.Enums;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Abstractions;
using static TradingView.Signals.Api.Controllers.SignalEventParser;

namespace TradingView.Signals.UnitTests
{
    public class Test1
    {
        private readonly ITestOutputHelper testOutputHelper;

        public Test1(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [Fact]
        public async Task Get()
        {
            var allText = await File.ReadAllTextAsync("response.json");

            var messages = JsonSerializer.Deserialize<List<SignalMessage>>(allText,
                new JsonSerializerOptions { PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance });

            var signalEvents = messages!
                .Select(x => TryParse(x.Desc, out var signalEvent) ? signalEvent : throw new Exception())
                .Skip(2)
                .SkipLast(1)
                .ToList();

            var list = signalEvents.GroupBy(x => x.Side).ToList();
        }

        public class SignalMessage
        {
            public long Id { get; set; }
            public int Aid { get; set; }
            public string Sym { get; set; }
            public string Res { get; set; }
            public string Desc { get; set; }
            public bool Snd { get; set; }
            public string SndFile { get; set; }
            public double SndDuration { get; set; }
            public bool Popup { get; set; }
            public int FireTime { get; set; }
            public int BarTime { get; set; }
            public bool CrossInt { get; set; }
            public string Name { get; set; }
        }

        public class SnakeCaseNamingPolicy : JsonNamingPolicy
        {
            private readonly SnakeCaseNamingStrategy newtonsoftSnakeCaseNamingStrategy
                = new();

            public static SnakeCaseNamingPolicy Instance { get; } = new();

            public override string ConvertName(string name) =>
                newtonsoftSnakeCaseNamingStrategy.GetPropertyName(name, false);
        }
    }
}
