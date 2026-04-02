using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Per-employee in-memory cache for consolidated WageType results (with retro-merge).
/// Pre-loaded once at PayrunEmployeeStart for all WageTypes tagged via the payroll
/// ClusterSet referenced by <c>Payroll.ClusterSet.ClusterSetWageTypeCons</c>.
/// Serves <c>GetConsolidatedWageTypeResults(periodMoment, ...)</c> calls from memory,
/// eliminating one DB round-trip per Cons-dependent WageType per employee.
///
/// The retro-merge is applied by the existing <c>GetConsolidatedWageTypeResults</c>
/// stored procedure at pre-load time — the cache stores the already-consolidated values.
/// Queries with <c>noRetro = true</c> always bypass the cache.
/// </summary>
public sealed class WageTypeConsCache
{
    // (wageTypeNumber, periodStart) → consolidated WageTypeResult
    private readonly Dictionary<(decimal Number, DateTime PeriodStart), WageTypeResult> cache = new();

    /// <summary>
    /// The period moment used for the pre-load query (typically <c>PayrunJob.CycleStart</c>).
    /// Used in <see cref="CanServe"/> to verify the query covers the same period range.
    /// </summary>
    public DateTime PeriodMoment { get; }

    /// <summary>The WageType numbers pre-loaded into this cache.</summary>
    public IReadOnlyCollection<decimal> WageTypeNumbers { get; }

    /// <summary>
    /// Initialises the cache from the bulk-loaded consolidated results.
    /// </summary>
    /// <param name="periodMoment">The period moment used for the pre-load query.</param>
    /// <param name="wageTypeNumbers">The WageType numbers that were pre-loaded.</param>
    /// <param name="results">The already-consolidated DB results to populate the cache from.</param>
    public WageTypeConsCache(DateTime periodMoment,
        IEnumerable<decimal> wageTypeNumbers, IEnumerable<WageTypeResult> results)
    {
        PeriodMoment = periodMoment;
        WageTypeNumbers = wageTypeNumbers.ToList().AsReadOnly();

        foreach (var r in results)
        {
            cache[(r.WageTypeNumber, r.Start)] = r;
        }
    }

    /// <summary>
    /// Returns <c>true</c> when this cache can fully serve the given query without a DB call.
    /// <list type="bullet">
    ///   <item><paramref name="noRetro"/> must be <c>false</c> — the cache always stores retro-merged values.</item>
    ///   <item><paramref name="periodMoment"/> must match the cache's <see cref="PeriodMoment"/> exactly.</item>
    ///   <item>All <paramref name="numbers"/> must be among the pre-loaded WageType numbers.</item>
    /// </list>
    /// </summary>
    public bool CanServe(IList<decimal> numbers, DateTime periodMoment, bool noRetro)
    {
        // noRetro queries need unmerged values — always bypass the cache
        if (noRetro)
        {
            return false;
        }

        // period moment must resolve to the same set of completed periods
        if (periodMoment != PeriodMoment)
        {
            return false;
        }

        return numbers.All(n => WageTypeNumbers.Contains(n));
    }

    /// <summary>
    /// Returns the cached consolidated results for the given WageType numbers.
    /// Only call after <see cref="CanServe"/> returns <c>true</c>.
    /// </summary>
    public IList<WageTypeResult> Get(IList<decimal> numbers)
    {
        var results = new List<WageTypeResult>();
        foreach (var number in numbers)
        {
            foreach (var kv in cache.Where(e => e.Key.Number == number))
            {
                results.Add(kv.Value);
            }
        }
        return results;
    }
}
