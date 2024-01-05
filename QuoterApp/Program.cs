using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using QuoterApp.BLL.Services;
using System.Threading.Tasks;

namespace QuoterApp;

class Program
{
    static async Task Main(string[] args)
    {
        var hostBuilder = CreateHostBuilder();
        await hostBuilder.RunConsoleAsync();
    }

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Other Services
                services.AddMemoryCache();

                // Application Services
                services.AddSingleton<IQuoter, YourQuoter>();
                services.AddSingleton<IMarketOrderSource, HardcodedMarketOrderSource>();

                services.AddHostedService<MarkerOrderSourceConsumer>();
                services.AddHostedService<AppLifetimeService>();
                services.AddSingleton<App>();

                // Change loging config
                services.AddLogging(config =>
                {
                    //add log filter for ConsoleLoggerProvider
                    config.AddFilter<ConsoleLoggerProvider>("Microsoft.Hosting.Lifetime", LogLevel.Error);
                });
            });
    }
}
