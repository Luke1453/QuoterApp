using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using QuoterApp.Common.Entities;

namespace QuoterApp.BLL.Tests;

public class QuoterTests
{
    private static readonly MarketOrder[] _marketOrders = new MarketOrder[] {
        new() {InstrumentId = "BA79603015", Price = 102.997, Quantity = 12 },
        new() {InstrumentId = "BA79603015", Price = 103.2, Quantity = 60 },
        new() {InstrumentId = "BA79603015", Price = 98.0, Quantity = 1 },
        new() {InstrumentId = "AB73567490", Price = 103.25, Quantity = 79 },
        new() {InstrumentId = "AB73567490", Price = 95.5, Quantity = 14 },
        new() {InstrumentId = "AB73567490", Price = 100.7, Quantity = 17 },
        new() {InstrumentId = "DK50782120", Price = 100.001, Quantity = 900 },
        new() {InstrumentId = "DK50782120", Price = 99.81, Quantity = 421 },
    };


    [TestCase("BA79603015", 50, true, 5152.364)]
    [TestCase("BA79603015", 50, false, 5160)]
    [TestCase("AB73567490", 50, true, 5010.65)]
    [TestCase("AB73567490", 50, false, 5162.5)]
    [TestCase("DK50782120", 50, true, 4990.5)]
    [TestCase("DK50782120", 50, false, 4990.5)]
    public void GetQuote_ShouldReturn_CorrectQuote(string instrumentId, int quantity, bool allowPartialFilling, double result)
    {
        // Arrange
        var _sut = new YourQuoter(MockMemoryCache());

        // Act
        var quote = _sut.GetQuote(instrumentId, quantity, allowPartialFilling);

        // Assert
        Assert.That(quote, Is.EqualTo(result));
    }

    [TestCase("DK50782120", 5000, true)]
    [TestCase("AB73567490", 5000, false)]
    [TestCase("BA79603015", 5000, true)]
    public void GetQuote_NoVolume_ShouldThrow(string instrumentId, int quantity, bool allowPartialFilling)
    {
        // Arrange
        var _sut = new YourQuoter(MockMemoryCache());

        // Act
        void test() => _sut.GetQuote(instrumentId, quantity, allowPartialFilling);

        // Assert
        var ex = Assert.Throws<ArgumentException>(test);
        Assert.That(ex?.Message.Contains("Can't fill quote, not enough volume in the market. The maximum amount is"), Is.True);
    }

    [TestCase("KD796030AA")]
    [TestCase("GKI9603033")]
    public void GetQuote_IncorrectInstrument_ShouldThrow(string instrumentId)
    {
        // Arrange
        var _sut = new YourQuoter(MockMemoryCache());

        // Act
        void test() => _sut.GetQuote(instrumentId, 1, true);

        // Assert
        var ex = Assert.Throws<ArgumentException>(test);
        Assert.That(ex?.Message, Is.EqualTo("Can't fill quote, not enough volume in the market. The maximum amount is 0 from 1 required"));
    }

    [TestCase("BA79603015", 103.09539726027397)]
    [TestCase("AB73567490", 101.86954545454545)]
    [TestCase("DK50782120", 99.940128690386075)]
    public void GetVolumeWeightedAveragePrice_ShouldReturn_CorrectQuotePrice(string instrumentId, double result)
    {
        // Arrange
        var _sut = new YourQuoter(MockMemoryCache());

        // Act
        var avgPrice = _sut.GetVolumeWeightedAveragePrice(instrumentId);

        // Assert
        Assert.That(avgPrice, Is.EqualTo(result));
    }

    [TestCase("KD796030AA")]
    [TestCase("GKI9603033")]
    public void GetVolumeWeightedAveragePrice_IncorrectInstrument_ShouldReturnZero(string instrumentId)
    {
        // Arrange
        var _sut = new YourQuoter(MockMemoryCache());

        // Act
        var avgPrice = _sut.GetVolumeWeightedAveragePrice(instrumentId);

        // Assert
        Assert.That(avgPrice, Is.EqualTo(.0));
    }


    #region Private Methods

    /// <summary>
    /// For mocking memory cache with predictable values before injecting into MyQuoter
    /// </summary>
    /// <returns></returns>
    private static IMemoryCache MockMemoryCache()
    {
        IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        foreach (var marketOrder in _marketOrders)
        {
            cache.Set(Guid.NewGuid().ToString(), marketOrder);
        }

        return cache;
    }

    #endregion
}
