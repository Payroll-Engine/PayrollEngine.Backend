//#define CASE_VALUE_LOAD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    private Dictionary<CaseValueKey, List<CaseValue>> caseValuesCache;
    private Dictionary<CaseValueKey, List<CaseValue>> casePeriodValuesCache;

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

        // setup cache
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

            caseValuesCache = new();
            var caseFieldValues = allCaseValues.GroupBy(x => new { ParentId, x.CaseFieldName, x.CaseSlot });
            foreach (var caseFieldValue in caseFieldValues)
            {
                var valueReference = CaseValueReference.ToReference(caseFieldValue.Key.CaseFieldName, caseFieldValue.Key.CaseSlot);
                caseValuesCache.Add(new(ParentId, valueReference), caseFieldValue.ToList());
            }
        }

        // cache lookup with case slot filtering
        var key = new CaseValueKey(ParentId, caseFieldName);
        if (!caseValuesCache.ContainsKey(key))
        {
            return new List<CaseValue>();
        }
        return caseValuesCache[key];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CaseValue>> GetCasePeriodValuesAsync(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // setup cache
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

            casePeriodValuesCache = new();
            var caseFieldValues = allCaseValues.GroupBy(x => x.GetCaseValueReference());
            foreach (var caseFieldValue in caseFieldValues)
            {
                var valueKey = new CaseValueKey(ParentId, caseFieldValue.Key);
                casePeriodValuesCache.TryAdd(valueKey, caseFieldValue.ToList());
            }

#if CASE_VALUE_LOAD
                stopwatch.Stop();
                PayrollEngine.Log.Information($"Load all case fields ({allCaseValues.Count} values): {stopwatch.ElapsedMilliseconds} ms");
#endif
        }

        // cache lookup with case slot filtering
        var key = new CaseValueKey(ParentId, caseFieldName);
        if (casePeriodValuesCache.TryGetValue(key, out var periodValues))
        {
            return periodValues;
        }
        return new List<CaseValue>();
    }

    /// <inheritdoc />
    public async Task<CaseValue> GetRetroCaseValueAsync(string caseFieldName, DatePeriod period) =>
        await CaseValueRepository.GetRetroCaseValueAsync(Context,
            new()
            {
                ParentId = ParentId,
                DivisionScope = DivisionScope.GlobalAndLocal,
                DivisionId = DivisionId,
                Forecast = Forecast,
            },
            period: period,
            caseFieldName: caseFieldName);

}