namespace TradingView.Signals.Api.Configurations
{
    public class BotConfiguration
    {
        public bool EnableTrading { get; set; }
    }

    public class ExchangeConfiguration
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}
