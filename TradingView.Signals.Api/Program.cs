using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TradingView.Signals.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Log.Information("Starting web host");

                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                // Log.Logger will likely be internal type "Serilog.Core.Pipeline.SilentLogger".
                if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
                {
                    // Loading configuration or Serilog failed.
                    // This will create a logger that can be captured by Azure logger.
                    // To enable Azure logger, in Azure Portal:
                    // 1. Go to WebApp
                    // 2. App Service logs
                    // 3. Enable "Application Logging (Filesystem)", "Application Logging (Filesystem)" and "Detailed error messages"
                    // 4. Set Retention Period (Days) to 10 or similar value
                    // 5. Save settings
                    // 6. Under Overview, restart web app
                    // 7. Go to Log Stream and observe the logs
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .CreateLogger();
                }

                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true)
                        .UseStartup<Startup>()
                        .UseSerilog((hostingContext, loggerConfiguration) =>
                        {
                            loggerConfiguration
                                .ReadFrom.Configuration(hostingContext.Configuration)
                                .Enrich.FromLogContext()
                                .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                                .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                                .Filter.ByExcluding(logEvent => logEvent.Exception is OperationCanceledException)
                                ;
#if DEBUG
                            // Used to filter out potentially bad data due debugging.
                            // Very useful when doing Seq dashboards and want to remove logs under debugging session.
                            loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
                        });
                });
    }
}
