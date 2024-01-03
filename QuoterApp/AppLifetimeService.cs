using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuoterApp;

public class AppLifetimeService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public AppLifetimeService(
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime applicationLifetime)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _applicationLifetime = applicationLifetime;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(OnStarted);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


    //Private Methods

    private void OnStarted()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            services.GetRequiredService<App>().Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.ReadLine();
            _applicationLifetime.StopApplication();
        }
    }
}
