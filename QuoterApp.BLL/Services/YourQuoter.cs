using Microsoft.Extensions.Caching.Memory;
using QuoterApp.Common.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QuoterApp;

public class YourQuoter : IQuoter
{
    private readonly IMemoryCache _cache;

    public YourQuoter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public double GetQuote(string instrumentId, int quantity, bool allowPartialFilling)
    {
        var marketOrders = GetCachedMarketOrders(instrumentId);

        int instrumentQuantity;
        if (allowPartialFilling)
        {
            // If partial filling allowed we can fill quote using multiple market orders
            instrumentQuantity = marketOrders.Sum(x => x.Quantity);
        }
        else
        {
            // If partial filling isn't allowed we can fill quote using only one market orders
            instrumentQuantity = marketOrders.Max(x => x.Quantity);
        }

        if (instrumentQuantity < quantity)
        {
            throw new ArgumentException($"Can't fill quote, not enough volume in the market. The maximum amount is {instrumentQuantity} from {quantity} required");
        }

        // Sort Market Orders by price
        marketOrders.Sort((x, y) => x.Price.CompareTo(y.Price));

        if (allowPartialFilling)
        {
            // If partial filling allowed we can fill quote using multiple market orders
            double quotePrice = 0;
            foreach (var marketOrder in marketOrders)
            {
                if (quantity <= marketOrder.Quantity)
                {
                    quotePrice += marketOrder.Price * quantity;
                    break;
                }
                else
                {
                    quotePrice += marketOrder.Price * marketOrder.Quantity;
                    quantity -= marketOrder.Quantity;
                }
            }

            return quotePrice;
        }
        else
        {
            // If partial filling isn't allowed we can fill quote using only one market orders
            foreach (var marketOrder in marketOrders)
            {
                if (quantity <= marketOrder.Quantity)
                {
                    return marketOrder.Price * quantity;
                }
            }

            throw new ArgumentException($"Can't fill quote, not enough volume in the market. The maximum amount is {instrumentQuantity} from {quantity} required");
        }
    }

    public double GetVolumeWeightedAveragePrice(string instrumentId)
    {
        var marketOrders = GetCachedMarketOrders(instrumentId);

        double avgPrice = 0.0;
        double volume = 0.0;
        marketOrders.ForEach(quote =>
        {
            avgPrice += quote.Quantity * quote.Price;
            volume += quote.Quantity;
        });

        // if volume = 0 -> avgProce = 0
        if (volume == 0)
        {
            return 0.0;
        }

        return avgPrice / volume;
    }


    #region Private Methods

    private List<MarketOrder> GetCachedMarketOrders(string instrumentID)
    {
        var marketOrders = new List<MarketOrder>();

        var coherentState = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
        var coherentStateValue = coherentState.GetValue(_cache);
        var entriesCollection = coherentStateValue.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
        var entriesCollectionValue = entriesCollection.GetValue(coherentStateValue) as ICollection;
        if (entriesCollectionValue != null)
        {
            foreach (var item in entriesCollectionValue)
            {
                var methodInfo = item.GetType().GetProperty("Value");
                var cacheValue = methodInfo.GetValue(item) as ICacheEntry;
                var marketOrder = cacheValue.Value as MarketOrder;

                if (marketOrder.InstrumentId == instrumentID)
                {
                    marketOrders.Add(marketOrder);
                }
            }
        }

        return marketOrders;
    }

    #endregion
}
