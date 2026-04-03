using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Per-employee in-memory cache for WageType cycle results.
/// Pre-loaded once at PayrunEmployeeStart for all WageTypes tagged via the payroll
/// ClusterSet referenced by <c>Payroll.ClusterSet.ClusterSetWageTypeCycle</c>.
/// Serves <c>GetWageTypeResults(cycleStart, previousPeriodEnd)</c> calls from memory,
/// eliminating one DB round-trip per qualifying WageType per employee.
/// Works for any calendar cycle type (annual, quarterly, monthly, etc.).
/// </summary>
public sealed class WageTypeCycleCache
{
    // (wageTypeNumber, periodStart) → WageTypeResult
    private readonly Dictionary<(decimal Number, DateTime PeriodStart), WageTypeResult> cache = new();

    /// <summary>The cycle start date this cache covers (inclusive lower bound).</summary>
    public DateTime CycleStart { get; }

    /// <summary>
    /// The inclusive upper bound of the pre-loaded date range.
    /// Equals the last day of the previous period (<c>PayrunJob.PeriodStart.AddDays(-1)</c>).
    /// </summary>
    public DateTime PreviousPeriodEnd { get; }

    /// <summary>The WageType numbers pre-loaded into this cache.</summary>
    public IReadOnlyCollection<decimal> WageTypeNumbers { get; }

    /// <summary>
    /// Initialises the cache from the bulk-loaded results.
    /// </summary>
    /// <param name="cycleStart">Inclusive lower bound of the covered date range.</param>
    /// <param name="previousPeriodEnd">Inclusive upper bound (<c>PeriodStart - 1 day</c>).</param>
    /// <param name="wageTypeNumbers">The WageType numbers that were pre-loaded.</param>
    /// <param name="results">The raw DB results to populate the cache from.</param>
    public WageTypeCycleCache(DateTime cycleStart, DateTime previousPeriodEnd,
        IEnumerable<decimal> wageTypeNumbers, IEnumerable<WageTypeResult> results)
    {
        CycleStart = cycleStart;
        PreviousPeriodEnd = previousPeriodEnd;
        WageTypeNumbers = wageTypeNumbers.ToList().AsReadOnly();

        foreach (var r in results)
        {
            cache[(r.WageTypeNumber, r.Start)] = r;
        }
    }

    /// <summary>
    /// Returns <c>true</c> when this cache can fully serve the given query without a DB call.
    /// The <paramref name="start"/>/<paramref name="end"/> range must match exactly and all
    /// requested numbers must be among the pre-loaded WageType numbers.
    /// </summary>
    public bool CanServe(IList<decimal> numbers, DateTime start, DateTime end)
    {
        if (start != CycleStart || end != PreviousPeriodEnd)
        {
            return false;
        }

        return numbers.All(n => WageTypeNumbers.Contains(n));
    }

    /// <summary>
    /// Returns the cached results for the given WageType numbers.
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
