//#define CASE_VALUE_LOAD
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Set with all repository case values
/// </summary>
public class CaseValueCache : ICaseValueCache
{
    private sealed class CaseValueKey : Tuple<int, string>
    {
        internal CaseValueKey(int parentId, string caseFieldName) :
            base(parentId, caseFieldName)
        {
        }
    }

    /// <summary>
    /// The database context
    /// </summary>
    private IDbContext Context { get; }

    /// <summary>
    /// The case value repository
    /// </summary>
    private ICaseValueRepository CaseValueRepository { get; }

    /// <summary>
    /// The repository parent id
    /// </summary>
    private int ParentId { get; }

    /// <summary>
    /// The division id
    /// </summary>
    private int DivisionId { get; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    private DateTime EvaluationDate { get; }

    /// <summary>
    /// The forecast name
    /// </summary>
    private string Forecast { get; }

    private ConcurrentDictionary<CaseValueKey, List<CaseValue>> caseValuesCache;
    private ConcurrentDictionary<CaseValueKey, List<CaseValue>> casePeriodValuesCache;

    // one semaphore per cache: serializes initialization, allows concurrent reads after first load
    private readonly SemaphoreSlim caseValuesCacheInitLock = new(1, 1);
    private readonly SemaphoreSlim casePeriodValuesCacheInitLock = new(1, 1);

    /// <summary>
    /// Case value cache constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseValueRepository"></param>
    /// <param name="parentId"></param>
    /// <param name="divisionId"></param>
    /// <param name="evaluationDate"></param>
    /// <param name="forecast"></param>
    public CaseValueCache(IDbContext context, ICaseValueRepository caseValueRepository, int parentId, int divisionId,
        DateTime evaluationDate, string forecast = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        CaseValueRepository = caseValueRepository ?? throw new ArgumentNullException(nameof(caseValueRepository));
        ParentId = parentId;
        DivisionId = divisionId;
        EvaluationDate = evaluationDate;
        Forecast = forecast;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCaseValueSlotsAsync(string caseFieldName) =>
        await CaseValueRepository.GetCaseValueSlotsAsync(Context, ParentId, caseFieldName);

    /// <inheritdoc />
    public async Task<IEnumerable<CaseValue>> GetCaseValuesAsync(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // setup cache – double-checked locking with SemaphoreSlim (async-safe)
        if (caseValuesCache == null)
        {
            await caseValuesCacheInitLock.WaitAsync();
            try
            {
                if (caseValuesCache == null)
                {
#if CASE_VALUE_LOAD
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
#endif

                    // all values until the evaluation date
                    var allCaseValues = (await CaseValueRepository.GetCaseValuesAsync(Context,
                        new()
                        {
                            ParentId = ParentId,
                            DivisionScope = DivisionScope.GlobalAndLocal,
                            DivisionId = DivisionId,
                            Forecast = Forecast,
                        },
                        evaluationDate: EvaluationDate)).ToList();

#if CASE_VALUE_LOAD
                    stopwatch.Stop();
                    PayrollEngine.Log.Information($"Load all case fields ({allCaseValues.Count} values): {stopwatch.ElapsedMilliseconds} ms");
#endif

                    var newCache = new ConcurrentDictionary<CaseValueKey, List<CaseValue>>();
                    var caseFieldValues = allCaseValues.GroupBy(x => new { ParentId, x.CaseFieldName, x.CaseSlot });
                    foreach (var caseFieldValue in caseFieldValues)
                    {
                        var valueReference = CaseValueReference.ToReference(caseFieldValue.Key.CaseFieldName, caseFieldValue.Key.CaseSlot);
                        newCache.TryAdd(new(ParentId, valueReference), caseFieldValue.ToList());
                    }
                    caseValuesCache = newCache;
                }
            }
            finally
            {
                caseValuesCacheInitLock.Release();
            }
        }

        // cache lookup with case slot filtering
        var key = new CaseValueKey(ParentId, new CaseValueReference(caseFieldName).Reference);
        return caseValuesCache.TryGetValue(key, out var caseValues) ? caseValues : [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CaseValue>> GetCasePeriodValuesAsync(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // setup cache – double-checked locking with SemaphoreSlim (async-safe)
        if (casePeriodValuesCache == null)
        {
            await casePeriodValuesCacheInitLock.WaitAsync();
            try
            {
                if (casePeriodValuesCache == null)
                {
                    // non forecasts: all values until the evaluation date
                    // forecasts: all values
                    var allValuesPeriod = string.IsNullOrWhiteSpace(Forecast) ?
                        new(Date.MinValue, EvaluationDate) : new DatePeriod();

#if CASE_VALUE_LOAD
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
#endif

                    var allCaseValues = (await CaseValueRepository.GetPeriodCaseValuesAsync(Context,
                        new()
                        {
                            ParentId = ParentId,
                            DivisionScope = DivisionScope.GlobalAndLocal,
                            DivisionId = DivisionId,
                            Forecast = Forecast,
                        },
                        period: allValuesPeriod,
                        evaluationDate: EvaluationDate)).ToList();

                    var newCache = new ConcurrentDictionary<CaseValueKey, List<CaseValue>>();
                    var caseFieldValues = allCaseValues.GroupBy(x => x.GetCaseValueReference());
                    foreach (var caseFieldValue in caseFieldValues)
                    {
                        var valueKey = new CaseValueKey(ParentId, caseFieldValue.Key);
                        newCache.TryAdd(valueKey, caseFieldValue.ToList());
                    }
                    casePeriodValuesCache = newCache;

#if CASE_VALUE_LOAD
                    stopwatch.Stop();
                    PayrollEngine.Log.Information($"Load all case fields ({allCaseValues.Count} values): {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
            }
            finally
            {
                casePeriodValuesCacheInitLock.Release();
            }
        }

        // cache lookup with case slot filtering
        var key = new CaseValueKey(ParentId, new CaseValueReference(caseFieldName).Reference);
        return casePeriodValuesCache.TryGetValue(key, out var periodValues) ? periodValues : [];
    }

    /// <inheritdoc />
    public async Task<CaseValue> GetRetroCaseValueAsync(string caseFieldName, DatePeriod period)
    {
        var allCaseValues = (await GetCaseValuesAsync(caseFieldName)).ToList();
        var caseValue = allCaseValues
            .Where(cv => // created within the period
                         (period.IsWithin(cv.Created) ||
                          // cancelled within the period
                          (cv.CancellationDate != null &&
                           period.IsWithin(cv.CancellationDate.Value)) &&
                          // division filter
                          (cv.DivisionId == null || cv.DivisionId == DivisionId) &&
                          // forecast filter
                          (string.IsNullOrWhiteSpace(Forecast) ?
                              string.IsNullOrWhiteSpace(cv.Forecast) :
                              string.Equals(Forecast, cv.Forecast))))
            // order from newest to oldest
            .OrderByDescending(x => x.Created)
            // oldest created
            .MinBy(x => x.Start);
        return caseValue;
    }
}