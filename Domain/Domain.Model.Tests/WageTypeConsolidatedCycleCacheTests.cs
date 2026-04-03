using System;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class WageTypeConsolidatedCycleCacheTests
{
    // -------------------------------------------------------------------------
    // Test data helpers
    // -------------------------------------------------------------------------

    private static readonly DateTime CycleStart = new(2026, 1, 1);   // typical periodMoment

    private static WageTypeResult MakeResult(decimal number, DateTime periodStart,
        decimal value = 100m, string name = null) =>
        new()
        {
            WageTypeNumber = number,
            WageTypeName   = name ?? $"WT{number}",
            Start          = periodStart,
            End            = periodStart.AddMonths(1).AddDays(-1),
            Value          = value
        };

    private static WageTypeConsolidatedCycleCache BuildCache(
        IEnumerable<decimal> numbers, IEnumerable<WageTypeResult> results,
        DateTime? periodMoment = null) =>
        new(periodMoment ?? CycleStart, numbers, results);

    // -------------------------------------------------------------------------
    // CanServe — noRetro guard
    // -------------------------------------------------------------------------

    [Fact]
    public void CanServe_ReturnsFalse_WhenNoRetroIsTrue()
    {
        var cache = BuildCache([5001m], [MakeResult(5001m, CycleStart)]);

        var result = cache.CanServe([5001m], CycleStart, noRetro: true);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsTrue_WhenNoRetroIsFalse()
    {
        var cache = BuildCache([5001m], [MakeResult(5001m, CycleStart)]);

        var result = cache.CanServe([5001m], CycleStart, noRetro: false);

        Assert.True(result);
    }

    // -------------------------------------------------------------------------
    // CanServe — period moment checks
    // -------------------------------------------------------------------------

    [Fact]
    public void CanServe_ReturnsFalse_WhenPeriodMomentDiffers()
    {
        var cache = BuildCache([5001m], [MakeResult(5001m, CycleStart)]);
        var differentMoment = CycleStart.AddMonths(1);

        var result = cache.CanServe([5001m], differentMoment, noRetro: false);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsTrue_WhenPeriodMomentMatches()
    {
        var cache = BuildCache([5001m], [MakeResult(5001m, CycleStart)]);

        var result = cache.CanServe([5001m], CycleStart, noRetro: false);

        Assert.True(result);
    }

    // -------------------------------------------------------------------------
    // CanServe — WageType number checks
    // -------------------------------------------------------------------------

    [Fact]
    public void CanServe_ReturnsFalse_WhenWageTypeNotPreloaded()
    {
        var cache = BuildCache([5001m], [MakeResult(5001m, CycleStart)]);

        var result = cache.CanServe([9999m], CycleStart, noRetro: false);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsFalse_WhenAnyRequestedNumberNotPreloaded()
    {
        var cache = BuildCache([5001m, 5002m],
        [
            MakeResult(5001m, CycleStart),
            MakeResult(5002m, CycleStart)
        ]);

        var result = cache.CanServe([5001m, 5002m, 9999m], CycleStart, noRetro: false);

        Assert.False(result);
    }

    [Fact]
    public void CanServe_ReturnsTrue_WhenRequestedNumbersAreSubsetOfPreloaded()
    {
        var cache = BuildCache([5001m, 5002m, 5004m],
        [
            MakeResult(5001m, CycleStart),
            MakeResult(5002m, CycleStart),
            MakeResult(5004m, CycleStart)
        ]);

        var result = cache.CanServe([5001m, 5002m], CycleStart, noRetro: false);

        Assert.True(result);
    }

    // -------------------------------------------------------------------------
    // Get — result retrieval
    // -------------------------------------------------------------------------

    [Fact]
    public void Get_ReturnsEmpty_WhenNoPriorPeriodResults()
    {
        var cache = BuildCache([5001m], []);

        var results = cache.Get([5001m]);

        Assert.Empty(results);
    }

    [Fact]
    public void Get_ReturnsSingleResult_ForSinglePeriod()
    {
        var expected = MakeResult(5001m, CycleStart, 2500m);
        var cache = BuildCache([5001m], [expected]);

        var results = cache.Get([5001m]);

        Assert.Single(results);
        Assert.Equal(expected.WageTypeNumber, results[0].WageTypeNumber);
        Assert.Equal(expected.Start, results[0].Start);
        Assert.Equal(expected.Value, results[0].Value);
    }

    [Fact]
    public void Get_ReturnsAllPeriodResults_ForMultiplePeriods()
    {
        var jan = MakeResult(5001m, new DateTime(2026, 1, 1), 2500m);
        var feb = MakeResult(5001m, new DateTime(2026, 2, 1), 2600m);
        var cache = BuildCache([5001m], [jan, feb]);

        var results = cache.Get([5001m]);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Get_ReturnsResultsForAllRequestedWageTypes()
    {
        var r1 = MakeResult(5001m, CycleStart, 100m);
        var r2 = MakeResult(5002m, CycleStart, 200m);
        var r3 = MakeResult(5004m, CycleStart, 300m);
        var cache = BuildCache([5001m, 5002m, 5004m], [r1, r2, r3]);

        var results = cache.Get([5001m, 5002m, 5004m]);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Get_ReturnsOnlyRequestedSubset()
    {
        var r1 = MakeResult(5001m, CycleStart, 100m);
        var r2 = MakeResult(5002m, CycleStart, 200m);
        var cache = BuildCache([5001m, 5002m], [r1, r2]);

        var results = cache.Get([5001m]);

        Assert.Single(results);
        Assert.Equal(5001m, results[0].WageTypeNumber);
    }

    [Fact]
    public void Get_PreservesOriginalResultObject()
    {
        var original = MakeResult(5001m, CycleStart, 999.99m, "CH.WageTypeOasi");
        original.Tags = ["retro", "legal"];
        var cache = BuildCache([5001m], [original]);

        var results = cache.Get([5001m]);

        Assert.Same(original, results[0]);
    }

    // -------------------------------------------------------------------------
    // Metadata properties
    // -------------------------------------------------------------------------

    [Fact]
    public void PeriodMoment_MatchesConstructorArgument()
    {
        var cache = BuildCache([5001m], []);

        Assert.Equal(CycleStart, cache.PeriodMoment);
    }

    [Fact]
    public void WageTypeNumbers_ContainsAllPreloadedNumbers()
    {
        var cache = BuildCache([5001m, 5002m, 5004m], []);

        Assert.Contains(5001m, cache.WageTypeNumbers);
        Assert.Contains(5002m, cache.WageTypeNumbers);
        Assert.Contains(5004m, cache.WageTypeNumbers);
        Assert.Equal(3, cache.WageTypeNumbers.Count);
    }
}
