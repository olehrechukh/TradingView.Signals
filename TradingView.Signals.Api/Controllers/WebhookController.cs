using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TradingView.Signals.Api.Strategy;
using TradingView.Signals.Api.Strategy.Models;

namespace TradingView.Signals.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly EventsChannelAsync<IExchangeEvent> channel;
        private readonly ILogger<WebhookController> logger;

        public WebhookController(EventsChannelAsync<IExchangeEvent> channel, ILogger<WebhookController> logger)
        {
            this.channel = channel;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> OnSignal()
        {
            var message = await GetMessage(Request);
            logger.LogInformation("New message: {value}", message);

            if (SignalEventParser.TryParse(message, out var signalEvent))
            {
                channel.AddEvent(signalEvent);
            }
            else
            {
                logger.LogError("Can not parse signal, {message}", message);
            }

            return Ok();
        }

        [HttpGet]
        public string Health()
        {
            logger.LogInformation("New message: {value}", nameof(Health));

            return "Now" + DateTime.UtcNow.ToLongTimeString();
        }

        private static async ValueTask<string> GetMessage(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            var content = await reader.ReadToEndAsync();
            return content;
        }
    }
}
