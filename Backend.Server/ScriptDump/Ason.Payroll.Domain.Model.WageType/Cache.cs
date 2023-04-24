/* Cache */
//#define CACHE_LOG
using System;
using System.Collections.Generic;
using System.Linq;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Payroll.Client.Scripting.Cache;


/// <summary>Result cache cycle</summary>
public enum ResultCacheCycle
{
    /// <summary>Cache previous cycle results too</summary>
    PreviousCycle,

    /// <summary>Cache current cycle results only</summary>
    CurrentCycle
}

/// <summary>Cache for collector results</summary>
public abstract class ConsolidatedResultCache
{
    /// <summary>The caching start date</summary>
    public DateTime CycleStartDate { get; }

    /// <summary>The result cache cycle</summary>
    public ResultCacheCycle CacheCycle { get; }

    /// <summary>The payrun job status</summary>
    public PayrunJobStatus? JobStatus { get; }

    /// <summary>Cache constructor</summary>
    /// <param name="cycleStartDate">The cycle start date</param>
    /// <param name="jobStatus">The payrun job status</param>
    /// <param name="cacheCycle">The result cache cycle</param>
    protected ConsolidatedResultCache(DateTime cycleStartDate,
        PayrunJobStatus? jobStatus = null,
        ResultCacheCycle cacheCycle = ResultCacheCycle.PreviousCycle)
    {
        CycleStartDate = cycleStartDate;
        JobStatus = jobStatus;
        CacheCycle = cacheCycle;
    }

    /// <summary>Get start date of previous cycle</summary>
    /// <param name="function">The payrun function</param>
    protected DateTime GetCacheStartDate(PayrunFunction function) =>
        CacheCycle switch
        {
            ResultCacheCycle.PreviousCycle => function.GetCycle(CycleStartDate, -1).Start,
            ResultCacheCycle.CurrentCycle => CycleStartDate,
            _ => throw new ArgumentOutOfRangeException()
        };

    /// <summary>Check for matching result period</summary>
    /// <param name="periodStarts">The period starts</param>
    /// <param name="resultStart">The result start date</param>
    /// <returns>True on matching period</returns>
    protected static bool IsMatchingPeriodStarts(IList<DateTime> periodStarts, DateTime resultStart)
        => periodStarts.Any(x => x.Date == resultStart.Date);

    /// <summary>Get consolidated period starts from the query moment until the current period</summary>
    /// <param name="function">The payrun function</param>
    /// <param name="query">The consolidated result query</param>
    /// <returns>The period start dates</returns>
    protected List<DateTime> GetConsolidatedPeriodStarts(PayrunFunction function, ConsolidatedResultQuery query)
    {
        // test for valid cycle setup
        if (function.CycleStart != CycleStartDate)
        {
            throw new ScriptException($"Mismatching cache cycle dates: {function.CycleStart} vs {CycleStartDate}");
        }

        // forecast
        if (!string.IsNullOrWhiteSpace(query.Forecast))
        {
            // forecast queries not supported for cached consolidated results
            return null;
        }

        // job status
        if (query.JobStatus != JobStatus)
        {
            if (!query.JobStatus.HasValue || !JobStatus.HasValue)
            {
                // mismatching job status
                return null;
            }
            // test matching job status by bit mask, duplicated in backend database stored procedure GetConsolidatedXxxResults
            if ((query.JobStatus.Value & JobStatus.Value) != query.JobStatus.Value)
            {
                return null;
            }
        }

        // periods: duplicated in backend domain scripting type PayrunRuntime
        var periodStarts = new List<DateTime>();
        var period = function.GetPeriod(query.PeriodMoment);
        // iterate from the start period until the job period
        while (period.Start < function.PeriodStart)
        {
            periodStarts.Add(period.Start);
            // next period
            period = function.GetPeriod(period.Start, 1);
        }
        var cycle = function.GetCycle(query.PeriodMoment);
        if (!periodStarts.All(x => x >= cycle.Start && x <= function.PeriodEnd))
        {
            return null;
        }

        // order periods from newest to oldest
        return new(periodStarts.OrderByDescending(x => x));
    }

    // Duplicated in backend payroll repository command
    /// <summary>Check for matching result tags</summary>
    /// <param name="queryTags">The query tags</param>
    /// <param name="resultTags">The result tags</param>
    /// <returns>True on matching tags</returns>
    protected static bool IsMatchingTags(IList<string> queryTags, IList<string> resultTags)
    {
        // build query tag list
        if (queryTags == null || !queryTags.Any())
        {
            return true;
        }
        var tagList = new List<string>(queryTags.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
        if (!tagList.Any())
        {
            return true;
        }

        // missing result tags
        if (resultTags == null || !resultTags.Any())
        {
            return false;
        }

        // first tag used to enable the logical OR mode
        var anyTag = "*".Equals(tagList.First().Trim(), StringComparison.InvariantCultureIgnoreCase);
        if (anyTag)
        {
            tagList.RemoveAt(0);
            // any query only with multiple tags
            anyTag = tagList.Count > 1;
        }
        if (!tagList.Any())
        {
            return false;
        }

        // test tags filter
        return anyTag ?
            // check if any tag is present
            tagList.Any(resultTags.Contains) :
            // check if all tags are present
            tagList.All(resultTags.Contains);
    }
}

/// <summary>Cache for consolidated collector results</summary>
public class CollectorConsolidatedResultCache : ConsolidatedResultCache
{
    /// <summary>The consolidated results cache</summary>
    private IList<CollectorResult> ConsolidatedResults { get; set; }

    /// <summary>Cache constructor</summary>
    /// <param name="cycleStartDate">The cycle start date</param>
    /// <param name="jobStatus">The payrun job status</param>
    /// <param name="cacheCycle">The result cache cycle</param>
    public CollectorConsolidatedResultCache(DateTime cycleStartDate,
        PayrunJobStatus? jobStatus = null,
        ResultCacheCycle cacheCycle = ResultCacheCycle.PreviousCycle) :
        base(cycleStartDate, jobStatus, cacheCycle)
    {
    }

    /// <summary>Get the collector results</summary>
    public IList<CollectorResult> GetConsolidatedResults(PayrunFunction function,
        CollectorConsolidatedResultQuery query, string runtimeKey = null)
    {
        IList<CollectorResult> result;
        var periodStarts = GetConsolidatedPeriodStarts(function, query);
        if (periodStarts != null && PrepareResultCache(function, runtimeKey))
        {
            // use cache
            result = ConsolidatedResults.Where(x =>
                        IsMatchingCollectorName(query.Collectors, x.CollectorName) &&
                        IsMatchingPeriodStarts(periodStarts, x.Start) &&
                        IsMatchingTags(query.Tags, x.Tags))
                    .ToList();
#if CACHE_LOG
            function.LogWarning($"CollectorResultCache: reusing cache ({result.Count})");
#endif
        }
        else
        {
            // fallback to slow backend request
            result = function.GetConsolidatedCollectorResults(query);
#if CACHE_LOG
            function.LogWarning($"CollectorResultCache: fallback to database query ({result.Count})");
#endif
        }
        return result;
    }

    private static bool IsMatchingCollectorName(IList<string> queryCollectors, string collectorName) =>
        !queryCollectors.Any() || queryCollectors.Contains(collectorName);

    /// <summary>Get the result cache from the employee runtime</summary>
    /// <param name="function"></param>
    /// <param name="runtimeKey">The employee runtime key, default is <see cref="CollectorConsolidatedResultCache"/></param>
    /// <returns>The collector result cache</returns>
    private bool PrepareResultCache(PayrunFunction function, string runtimeKey = null)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }
        if (string.IsNullOrWhiteSpace(runtimeKey))
        {
            runtimeKey = nameof(CollectorConsolidatedResultCache);
        }

        // reuse cached results
        if (ConsolidatedResults != null)
        {
            return true;
        }

        try
        {
            // cached values from employee runtime
            var results = function.GetEmployeeRuntimeValue<List<CollectorResult>>(runtimeKey);
            if (results == null || !results.Any())
            {
                var cacheStartDate = GetCacheStartDate(function);
                // initialize cache
                var query = new CollectorConsolidatedResultQuery(new List<string>(), cacheStartDate, JobStatus);
                ConsolidatedResults = function.GetConsolidatedCollectorResults(query);

#if CACHE_LOG
                function.LogWarning($"CollectorResultCache:: setup cache: start={cacheStartDate}, period={function.Period}, {ConsolidatedResults?.Count}");
#endif

                // update employee runtime
                if (ConsolidatedResults != null && ConsolidatedResults.Any())
                {
                    function.SetEmployeeRuntimeValue(runtimeKey,
                        System.Text.Json.JsonSerializer.Serialize(ConsolidatedResults));
                }
            }
            else
            {
                // reused from employee runtime
                ConsolidatedResults = results;
#if CACHE_LOG
                function.LogWarning($"CollectorResultCache: consolidated collector cache success: {ConsolidatedResults?.Count}");
#endif
            }
        }
        catch (Exception exception)
        {
            function.LogError(exception);
            return false;
        }
        return true;
    }
}

/// <summary>Cache for consolidated wage type results</summary>
public class WageTypeConsolidatedResultCache : ConsolidatedResultCache
{
    /// <summary>The consolidated results cache</summary>
    private IList<WageTypeResult> ConsolidatedResults { get; set; }

    /// <summary>Cache constructor</summary>
    /// <param name="cycleStartDate">The cycle start date</param>
    /// <param name="jobStatus">The payrun job status</param>
    /// <param name="cacheCycle">The result cache cycle</param>
    public WageTypeConsolidatedResultCache(DateTime cycleStartDate,
        PayrunJobStatus? jobStatus = null,
        ResultCacheCycle cacheCycle = ResultCacheCycle.PreviousCycle) :
        base(cycleStartDate, jobStatus, cacheCycle)
    {
    }

    /// <summary>Get the collector results</summary>
    public IList<WageTypeResult> GetConsolidatedResults(PayrunFunction function,
        WageTypeConsolidatedResultQuery query, string runtimeKey = null)
    {
        IList<WageTypeResult> result;
        var periodStarts = GetConsolidatedPeriodStarts(function, query);

        // use cache
        if (periodStarts != null && PrepareResultCache(function, runtimeKey))
        {
            result = ConsolidatedResults.Where(x =>
                    IsMatchingWageTypeNumber(query.WageTypes, x.WageTypeNumber) &&
                    IsMatchingPeriodStarts(periodStarts, x.Start) &&
                    IsMatchingTags(query.Tags, x.Tags))
                .ToList();
#if CACHE_LOG
            function.LogWarning($"WageTypeResultCache: reusing cache ({result.Count})");
#endif
        }
        else
        {
            // fallback to slow backend request
            result = function.GetConsolidatedWageTypeResults(query);
#if CACHE_LOG
            function.LogWarning($"WageTypeResultCache: fallback to database query ({result.Count})");
#endif
        }
        return result;
    }

    private static bool IsMatchingWageTypeNumber(IList<decimal> queryNumbers, decimal wageTypeNumber) =>
        !queryNumbers.Any() || queryNumbers.Any(x => x == wageTypeNumber);

    /// <summary>Get the result cache from the employee runtime</summary>
    /// <param name="function"></param>
    /// <param name="runtimeKey">The employee runtime key, default is <see cref="WageTypeConsolidatedResultCache"/></param>
    /// <returns>The collector result cache</returns>
    private bool PrepareResultCache(PayrunFunction function, string runtimeKey = null)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }
        if (string.IsNullOrWhiteSpace(runtimeKey))
        {
            runtimeKey = nameof(WageTypeConsolidatedResultCache);
        }

        // reuse cached results
        if (ConsolidatedResults != null)
        {
            return true;
        }

        try
        {
            // cached values from employee runtime
            var results = function.GetEmployeeRuntimeValue<List<WageTypeResult>>(runtimeKey);
            if (results == null || !results.Any())
            {
                var cacheStartDate = GetCacheStartDate(function);
                // initialize cache
                var query = new WageTypeConsolidatedResultQuery(new List<decimal>(),
                    cacheStartDate, PayrunJobStatus.Legal);
                ConsolidatedResults = function.GetConsolidatedWageTypeResults(query);

#if CACHE_LOG
                function.LogWarning($"WageTypeResultCache:: setup cache: start={cacheStartDate}, period={function.Period}, {ConsolidatedResults?.Count}");
#endif

                // update employee runtime
                if (ConsolidatedResults != null && ConsolidatedResults.Any())
                {
                    function.SetEmployeeRuntimeValue(runtimeKey,
                        System.Text.Json.JsonSerializer.Serialize(ConsolidatedResults));
                }
            }
            else
            {
                ConsolidatedResults = results;
#if CACHE_LOG
                function.LogWarning($"WageTypeResultCache:: consolidated wage type cache success: {ConsolidatedResults?.Count}");
#endif
            }
        }
        catch (Exception exception)
        {
            function.LogError(exception);
            return false;
        }
        return true;
    }
}
