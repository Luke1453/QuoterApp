using System;
using System.Threading.Tasks;

namespace QuoterApp;

public class App
{
    private readonly IQuoter _quoter;
    private readonly string[] Instruments = { "BA79603015", "AB73567490", "DK50782120" };

    public App(IQuoter quoter)
    {
        _quoter = quoter;
    }


    public async Task Run()
    {
        await Task.Delay(2000);

        var rnd = new Random();
        for (int i = 0; i < 6; i++)
        {
            string instrumentID = Instruments[i % 3];
            bool allowPartialFilling = i % 2 == 0;
            int qty = rnd.Next(1000);

            var vwap = _quoter.GetVolumeWeightedAveragePrice(instrumentID);
            Console.WriteLine($"{instrumentID} - Current Volume Weighted Average Price: {vwap}");

            var partialFillingText = (allowPartialFilling ? "" : "Not ") + "Allow Partial Filling";
            double quote;
            try
            {
                quote = _quoter.GetQuote(instrumentID, qty, allowPartialFilling);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{instrumentID} ({partialFillingText}) - {ex.Message}");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine($"{instrumentID} ({partialFillingText}) - Quantity: {qty}, Quote: {quote}, Average: {quote / qty}");
            Console.WriteLine();
        }

        Console.WriteLine($"Done");
    }
}
