using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuoterApp.BLL.Services;

public class MarkerOrderSourceConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MarkerOrderSourceConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }


    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await ConsumeMarketOrderData(ct);
    }

    private async Task ConsumeMarketOrderData(CancellationToken ct)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var cache = scope.ServiceProvider.GetService<IMemoryCache>();
        var marketOrderSource = scope.ServiceProvider.GetService<IMarketOrderSource>();

        // Adding 3 market orders to seed the cache
        // Simmulate existing market orders
        for (int i = 0; i < 3; i++)
        {
            cache.Set(Guid.NewGuid().ToString(), await marketOrderSource.GetNextMarketOrder(), new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(MarketOrderExpiration())
            });
        }

        // Loop which will generate random Market Orders
        while (!ct.IsCancellationRequested)
        {
            cache.Set(Guid.NewGuid().ToString(), await marketOrderSource.GetNextMarketOrder(), new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(MarketOrderExpiration())
            });

            //await Task.Delay(500, ct); // Simulates delay in getting next quote
        }
    }

    //private Task<MarketOrder> GetMarket()
    //{
    //    Task.Run(() => { marketOrderSource.GetNextMarketOrder(), })
    //}

    private static TimeSpan MarketOrderExpiration()
    {
        // Generate random TimeSpan from 0.25s to 1s after which Merker Order is expired
        Random rnd = new();
        int randomMilliseconds = rnd.Next(250, 1000);
        return TimeSpan.FromMilliseconds(randomMilliseconds);
    }
}
