using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TradingView.Signals.Api.Strategy;

namespace TradingView.Signals.Api.Services
{
    public class RunnerStarter : BackgroundService
    {
        private readonly Runner runner;

        public RunnerStarter(Runner runner) => this.runner = runner;

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => runner.Run(stoppingToken);
    }
}
