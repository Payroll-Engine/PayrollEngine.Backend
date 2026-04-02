using System;
using System.Collections.Generic;
using PayrollEngine.Domain.Model.Repository;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class WageTypeYtdCacheTests
{
    // -------------------------------------------------------------------------
    // Test data helpers
    // -------------------------------------------------------------------------

    private static readonly DateTime CycleStart = new(2026, 1, 1);
    private static readonly DateTime PreviousPeriodEnd = new(2026, 2, 28); // March payrun → prev end = Feb 28

    private static WageTypeResult MakeResult(decimal number, DateTime periodStart, decimal value = 100m) =>
        new()
        {
            WageTypeNumber = number,
            WageTypeName = $"WT{number}",
            Start = periodStart,
            End = periodStart.AddMonths(1).AddDays(-1),
            Value = value
        };

    private static WageTypeYtdCache BuildCache(
        IEnumerable<decimal> numbers, IEnumerable<WageTypeResult> results) =>
        new(CycleStart, PreviousPeriodEnd, numbers, results);

    // -------------------------------------------------------------------------
    // CanServe — range checks
    // -------------------------------------------------------------------------

    [Fact]
    public void CanServe_ReturnsFalse_WhenStartDoesNotMatchCycleStart()
    {
        var cache = BuildCache([5150m], [MakeResult(5150m, CycleStart)]);
        var wrongStart = CycleStart.AddDays(1);

        var result = cache.CanServe([5150m], wrongStart, PreviousPeriodEnd);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsFalse_WhenEndDoesNotMatchPreviousPeriodEnd()
    {
        var cache = BuildCache([5150m], [MakeResult(5150m, CycleStart)]);
        var wrongEnd = PreviousPeriodEnd.AddDays(1);

        var result = cache.CanServe([5150m], CycleStart, wrongEnd);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsFalse_WhenWageTypeNotPreloaded()
    {
        var cache = BuildCache([5150m], [MakeResult(5150m, CycleStart)]);

        // 8100m was never loaded into this cache
        var result = cache.CanServe([8100m], CycleStart, PreviousPeriodEnd);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsFalse_WhenAnyRequestedNumberNotPreloaded()
    {
        var cache = BuildCache([5150m, 8100m],
        [
            MakeResult(5150m, CycleStart),
            MakeResult(8100m, CycleStart)
        ]);

        // 9999m was never loaded
        var result = cache.CanServe([5150m, 8100m, 9999m], CycleStart, PreviousPeriodEnd);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsTrue_WhenExactMatch()
    {
        var cache = BuildCache([5150m, 8100m],
        [
            MakeResult(5150m, CycleStart),
            MakeResult(8100m, CycleStart)
        ]);

        var result = cache.CanServe([5150m, 8100m], CycleStart, PreviousPeriodEnd);

        Assert.True(result);
    }

    [Fact]
    public void CanServe_ReturnsTrue_WhenRequestedNumbersAreSubsetOfPreloaded()
    {
        // cache has three WTs; query only asks for two — still a cache hit
        var cache = BuildCache([5150m, 8100m, 5100m],
        [
            MakeResult(5150m, CycleStart),
            MakeResult(8100m, CycleStart),
            MakeResult(5100m, CycleStart)
        ]);

        var result = cache.CanServe([5150m, 8100m], CycleStart, PreviousPeriodEnd);

        Assert.True(result);
    }

    // -------------------------------------------------------------------------
    // Get — result retrieval
    // -------------------------------------------------------------------------

    [Fact]
    public void Get_ReturnsEmpty_WhenNoResultsStoredForNumber()
    {
        // cache was built with the number but no DB rows exist (e.g. first payrun of the year)
        var cache = BuildCache([5150m], []);

        var results = cache.Get([5150m]);

        Assert.Empty(results);
    }

    [Fact]
    public void Get_ReturnsSingleResult_ForSinglePeriod()
    {
        var expected = MakeResult(5150m, CycleStart, 1234.56m);
        var cache = BuildCache([5150m], [expected]);

        var results = cache.Get([5150m]);

        Assert.Single(results);
        Assert.Equal(expected.WageTypeNumber, results[0].WageTypeNumber);
        Assert.Equal(expected.Start, results[0].Start);
        Assert.Equal(expected.Value, results[0].Value);
    }

    [Fact]
    public void Get_ReturnsAllPeriodResults_ForSingleWageType()
    {
        // two prior periods (Jan + Feb results for a March payrun)
        var jan = MakeResult(5150m, new DateTime(2026, 1, 1), 500m);
        var feb = MakeResult(5150m, new DateTime(2026, 2, 1), 600m);
        var cache = BuildCache([5150m], [jan, feb]);

        var results = cache.Get([5150m]);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Get_ReturnsResultsForAllRequestedWageTypes()
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        var r1 = MakeResult(5150m, CycleStart, 100m);
        var r2 = MakeResult(8100m, CycleStart, 200m);
        var r3 = MakeResult(5100m, CycleStart, 300m);
        var cache = BuildCache([5150m, 8100m, 5100m], [r1, r2, r3]);

        var results = cache.Get([5150m, 8100m, 5100m]);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Get_ReturnsOnlyRequestedWageTypes_WhenSubsetRequested()
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        var r1 = MakeResult(5150m, CycleStart, 100m);
        var r2 = MakeResult(8100m, CycleStart, 200m);
        var cache = BuildCache([5150m, 8100m], [r1, r2]);

        var results = cache.Get([5150m]);

        Assert.Single(results);
        Assert.Equal(5150m, results[0].WageTypeNumber);
    }

    [Fact]
    public void Get_PreservesOriginalResultObject()
    {
        var original = MakeResult(5150m, CycleStart, 777.77m);
        original.Tags = ["tag1", "tag2"];
        var cache = BuildCache([5150m], [original]);

        var results = cache.Get([5150m]);

        // must be the exact same object — no copying
        Assert.Same(original, results[0]);
    }

    // -------------------------------------------------------------------------
    // Metadata properties
    // -------------------------------------------------------------------------

    [Fact]
    public void CycleStart_MatchesConstructorArgument()
    {
        var cache = BuildCache([5150m], []);

        Assert.Equal(CycleStart, cache.CycleStart);
    }

    [Fact]
    public void PreviousPeriodEnd_MatchesConstructorArgument()
    {
        var cache = BuildCache([5150m], []);

        Assert.Equal(PreviousPeriodEnd, cache.PreviousPeriodEnd);
    }

    [Fact]
    public void WageTypeNumbers_ContainsAllPreloadedNumbers()
    {
        var cache = BuildCache([5150m, 8100m, 5100m], []);

        Assert.Contains(5150m, cache.WageTypeNumbers);
        Assert.Contains(8100m, cache.WageTypeNumbers);
        Assert.Contains(5100m, cache.WageTypeNumbers);
        Assert.Equal(3, cache.WageTypeNumbers.Count);
    }
}
