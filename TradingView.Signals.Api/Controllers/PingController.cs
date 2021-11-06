using Microsoft.AspNetCore.Mvc;

namespace TradingView.Signals.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PingController
    {
        [HttpGet]
        public string Ping() => "Pong";
    }
}
