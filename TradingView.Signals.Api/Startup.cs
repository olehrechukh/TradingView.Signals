using System.Globalization;
using System.Net.Http;
using System.Threading;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using TradingView.Signals.Api.Configurations;
using TradingView.Signals.Api.Services;
using TradingView.Signals.Api.Strategy;
using TradingView.Signals.Api.Strategy.Models;

namespace TradingView.Signals.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TradingView.Signals.Api", Version = "v1" });
            });

            services.AddHostedService<RunnerStarter>();
            services.AddRunner(Configuration);
            services.AddHttpClient();
            services.AddHealthChecks();
            services.AddRouting(options => options.LowercaseUrls = true);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradingView.Signals.Api v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/");
                endpoints.MapControllers();
            });
        }
    }

    public static class Ext
    {
        public static IServiceCollection AddRunner(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<EventsChannelAsync<IExchangeEvent>>();
            services.AddSingleton<Runner>();

            services.AddOptions<ExchangeConfiguration>().Bind(configuration.GetSection("Exchange"));
            services.AddOptions<BotConfiguration>().Bind(configuration.GetSection("Bot"));

            services.AddTransient(provider =>
            {
                var options = provider.GetRequiredService<IOptions<ExchangeConfiguration>>();
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = factory.CreateClient();

                return new BinanceClient(new BinanceClientOptions(httpClient)
                {
                    ApiCredentials = new ApiCredentials(options.Value.ApiKey, options.Value.ApiSecret)
                });
            });

            return services;
        }
    }
}
