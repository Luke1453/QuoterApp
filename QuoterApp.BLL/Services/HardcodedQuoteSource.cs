using QuoterApp.Common.Entities;
using System;
using System.Threading.Tasks;

namespace QuoterApp;

public class HardcodedMarketOrderSource : IMarketOrderSource
{
    private readonly string[] Instruments = { "BA79603015", "AB73567490", "DK50782120" };

    public async Task<MarketOrder> GetNextMarketOrder()
    {
        Random random = new();
        int instrumentIndex = random.Next(3);
        double price = random.NextDouble() * 150;
        int quantity = random.Next(1000);

        await Task.Delay(500);

        return new MarketOrder { InstrumentId = Instruments[instrumentIndex], Price = price, Quantity = quantity };
    }
}
