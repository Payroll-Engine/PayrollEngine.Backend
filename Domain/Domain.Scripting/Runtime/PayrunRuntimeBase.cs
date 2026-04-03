//#define SCRIPT_RESULT_REQUESTS
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting.Runtime;

// ReSharper disable InconsistentlySynchronizedField

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// runtime for the payrun script
/// </summary>
public abstract class PayrunRuntimeBase : PayrollRuntimeBase, IPayrunRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected new PayrunRuntimeSettings Settings => base.Settings as PayrunRuntimeSettings;

    /// <summary>The Payrun</summary>
    private Payrun Payrun => Settings.Payrun;

    /// <summary>
    /// Provider for regulation
    /// </summary>
    private IRegulationProvider RegulationProvider => Settings.RegulationProvider;

    /// <summary>
    /// Provider for employee results
    /// </summary>
    private IResultProvider ResultProvider => Settings.ResultProvider;

    /// <summary>
    /// Provider for runtime values
    /// </summary>
    protected IRuntimeValueProvider RuntimeValueProvider => Settings.RuntimeValueProvider;

    /// <inheritdoc />
    protected PayrunRuntimeBase(PayrunRuntimeSettings settings) :
        base(settings)
    {
    }

    #region Internal

    /// <summary>Function execution timeout <see cref="BackendScriptingSpecification.PayrunScriptFunctionTimeout"/></summary>
    protected override TimeSpan Timeout =>
        TimeSpan.FromMilliseconds(BackendScriptingSpecification.PayrunScriptFunctionTimeout);

    /// <summary>The log owner type</summary>
    protected override string LogOwner => PayrunName;

    /// <summary>The payrun results</summary>
    internal List<PayrunResult> PayrunResults { get; } = [];

    #endregion

    #region Payrun

    /// <inheritdoc />
    public int PayrunId => Payrun.Id;

    /// <inheritdoc />
    public string PayrunName => Payrun.Name;

    /// <inheritdoc />
    public int ExecutionPhase => (int)Settings.ExecutionPhase;

    #endregion

    #region PayrunJob

    /// <summary>The Payrun job</summary>
    private PayrunJob PayrunJob => Settings.PayrunJob;

    /// <summary>The parent payrun job, usually the payrun retro source payrun job</summary>
    private PayrunJob ParentPayrunJob => Settings.ParentPayrunJob;

    /// <inheritdoc />
    public Tuple<DateTime, DateTime> RetroPeriod =>
        ParentPayrunJob != null
            ? new Tuple<DateTime, DateTime>(ParentPayrunJob.PeriodStart, ParentPayrunJob.PeriodEnd)
            : null;

    /// <inheritdoc />
    public string Forecast => PayrunJob.Forecast;

    /// <inheritdoc />
    public string CycleName => PayrunJob.CycleName;

    /// <inheritdoc />
    public string PeriodName => PayrunJob.PeriodName;

    /// <inheritdoc />
    public bool PreviewJob => Settings.PreviewJob;

    // lock for thread-safe access to PayrunJob.Attributes (shared across parallel employee threads)
    private static readonly Lock PayrunJobAttributeLock = new();

    /// <inheritdoc />
    public object GetPayrunJobAttribute(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        lock (PayrunJobAttributeLock)
        {
            return PayrunJob.Attributes?.GetValue<object>(attributeName);
        }
    }

    /// <inheritdoc />
    public void SetPayrunJobAttribute(string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        lock (PayrunJobAttributeLock)
        {
            PayrunJob.Attributes ??= new();
            PayrunJob.Attributes[attributeName] = value;
        }
    }

    /// <inheritdoc />
    public bool RemovePayrunJobAttribute(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        lock (PayrunJobAttributeLock)
        {
            return PayrunJob.Attributes != null && PayrunJob.Attributes.Remove(attributeName);
        }
    }

    #endregion

    #region Payrun Runtime Values

    /// <inheritdoc />
    public bool HasPayrunRuntimeValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        return RuntimeValueProvider.PayrunValues.ContainsKey(key);
    }

    /// <inheritdoc />
    public string GetPayrunRuntimeValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        return RuntimeValueProvider.PayrunValues.GetValueOrDefault(key);
    }

    /// <inheritdoc />
    public void SetPayrunRuntimeValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        if (value == null)
        {
            // remove value
            RuntimeValueProvider.PayrunValues.TryRemove(key, out _);
        }
        else
        {
            RuntimeValueProvider.PayrunValues[key] = value;
        }
    }

    #endregion

    #region Employee Runtime Values

    public bool HasEmployeeRuntimeValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        var employee = Employee.Identifier;
        return RuntimeValueProvider.EmployeeValues.TryGetValue(employee, out var values) &&
               values.ContainsKey(key);
    }

    /// <inheritdoc />
    public string GetEmployeeRuntimeValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        var employee = Employee.Identifier;
        if (!RuntimeValueProvider.EmployeeValues.TryGetValue(employee, out var employeeValues))
        {
            return null;
        }
        return employeeValues.GetValueOrDefault(key);
    }

    /// <inheritdoc />
    public void SetEmployeeRuntimeValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }

        var employee = Employee.Identifier;
        var employeeValues = RuntimeValueProvider.EmployeeValues.GetOrAdd(employee, _ => new());
        if (value == null)
        {
            // remove value
            employeeValues.TryRemove(key, out _);
        }
        else
        {
            employeeValues[key] = value;
        }
    }

    #endregion

    #region Payrun Results

    /// <inheritdoc />
    public object GetPayrunResult(string source, string name)
    {
        var result = FindPayrollResult(source, name);
        if (string.IsNullOrWhiteSpace(result?.Value))
        {
            return null;
        }
        return ValueConvert.ToValue(result.Value, result.ValueType, new CultureInfo(result.Culture));
    }

    /// <inheritdoc />
    public void SetPayrunResult(string source, string name, object value, int valueType,
        DateTime startDate, DateTime endDate, string slot, List<string> tags,
        Dictionary<string, object> attributes, string culture)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(nameof(source));
        }
        if (startDate >= endDate)
        {
            throw new ArgumentException($"Invalid start date {startDate} on end {endDate}.");
        }

        if (value == null)
        {
            return;
        }

        // value
        var stringValue = value.ToString();

        // update existing
        var existing = FindPayrollResult(source, name);
        if (existing != null)
        {
            existing.Value = stringValue;
            existing.NumericValue = ValueConvert.ToNumber(
                json: stringValue,
                valueType: (ValueType)valueType,
                culture: CultureInfo.GetCultureInfo(culture));
            return;
        }

        // ensure attributes collection
        attributes ??= new();

        // value type
        if (!Enum.IsDefined(typeof(ValueType), valueType))
        {
            throw new ArgumentException($"Unknown value type: {valueType}.");
        }

        // culture
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = GetDerivedCulture(DivisionId, Employee.Id);
        }

        // result
        var result = new PayrunResult
        {
            Source = source,
            Name = name,
            // currently no support for localized custom wage type results
            Slot = slot,
            ValueType = (ValueType)valueType,
            Value = stringValue,
            NumericValue = ValueConvert.ToNumber(
                json: stringValue,
                valueType: (ValueType)valueType,
                culture: CultureInfo.GetCultureInfo(culture)),
            Culture = culture,
            Start = startDate,
            End = endDate,
            Tags = tags,
            Attributes = attributes
        };
        PayrunResults.Add(result);
    }

    private PayrunResult FindPayrollResult(string source, string name) =>
        PayrunResults.FirstOrDefault(
            x => string.Equals(x.Source, source) && string.Equals(x.Name, name));

    #endregion

    #region Wage Type

    /// <inheritdoc />
    public decimal GetWageTypeNumber(string wageTypeName)
    {
        if (string.IsNullOrWhiteSpace(wageTypeName))
        {
            throw new ArgumentException(nameof(wageTypeName));
        }

        // namespace
        wageTypeName = wageTypeName.EnsureNamespace(Settings.Namespace);

        foreach (var derivedWageType in RegulationProvider.DerivedWageTypes)
        {
            var wageType = derivedWageType.FirstOrDefault();
            if (wageType != null && string.Equals(wageType.Name, wageTypeName))
            {
                return wageType.WageTypeNumber;
            }
        }
        return 0;
    }

    /// <inheritdoc />
    public string GetWageTypeName(decimal wageTypeNumber)
    {
        var wageType = RegulationProvider.DerivedWageTypes.FirstOrDefault(x => x.Key == wageTypeNumber);
        return wageType?.FirstOrDefault()?.Name;
    }

    /// <inheritdoc />
    public IList<Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetWageTypeResults(
        IList<decimal> wageTypeNumbers, DateTime start, DateTime end, string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        var wageTypeResults = GetWageTypeResultsInternal(wageTypeNumbers, start, end, forecast, jobStatus, tags);
        var results = wageTypeResults.Select(x => new Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
            x.WageTypeNumber, x.WageTypeName, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();
#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Wage Type {wageTypeNumber} (range={new DatePeriod(start, end)}) results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return results;
    }

    /// <inheritdoc />
    public IList<Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetConsolidatedWageTypeResults(
        IList<decimal> wageTypeNumbers, DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null, bool noRetro = false)
    {
        if (wageTypeNumbers == null)
        {
            throw new ArgumentNullException(nameof(wageTypeNumbers));
        }

        var consolidatedResults = new List<Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>>();
        if (EmployeeId != null)
        {
            // serve from pre-loaded consolidated cache when available and query matches
            var consCache = Settings.WageTypeConsCache;
            if (consCache != null && consCache.CanServe(wageTypeNumbers, periodMoment, noRetro))
            {
                return consCache.Get(wageTypeNumbers)
                    .Select(x => new Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
                        x.WageTypeNumber, x.WageTypeName, new(x.Start, x.End), x.Value, x.Tags, x.Attributes))
                    .ToList();
            }

            var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
            if (periodStarts.Any() && EmployeeId != null)
            {
                var results = ResultProvider.GetConsolidatedWageTypeResultsAsync(Settings.DbContext,
                    new()
                    {
                        TenantId = TenantId,
                        EmployeeId = EmployeeId.Value,
                        DivisionId = PayrunJob.DivisionId,
                        WageTypeNumbers = wageTypeNumbers,
                        PeriodStarts = periodStarts,
                        Forecast = forecast,
                        Tags = tags,
                        JobStatus = (PayrunJobStatus?)jobStatus,
                        EvaluationDate = EvaluationDate,
                        NoRetro = noRetro,
                        // For retro-jobs: exclude sibling retro-jobs of the current payrun so that
                        // consolidated results reflect the previously completed payslip state.
                        // ParentPayrunJob is the current payrun's full-job; its id is the ParentJobId
                        // of all retro-jobs in this payrun. Retro-jobs from earlier payruns are unaffected.
                        ExcludeParentJobId = ParentPayrunJob?.Id,
                    }).GetAwaiter().GetResult()
                    .Select(x => new Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
                    x.WageTypeNumber, x.WageTypeName, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();

                // collect results
                consolidatedResults.AddRange(results);
            }
        }
        else
        {
            throw new PayrollException("Missing employee on consolidated collector result request.");
        }


#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Wage Type {wageTypeNumber} (moment={periodMoment}) consolidated results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return consolidatedResults;
    }

    /// <inheritdoc />
    public IList<Tuple<decimal, string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetWageTypeCustomResults(IList<decimal> wageTypeNumbers, DateTime start, DateTime end, string forecast = null,
        int? jobStatus = null, IList<string> tags = null)
    {
        var customResults = GetWageTypeCustomResultsInternal(wageTypeNumbers, start, end, forecast, jobStatus, tags);
        var results = customResults.Select(x => new Tuple<decimal, string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
            x.WageTypeNumber, x.WageTypeName, x.Source, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();
#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Wage Type {wageTypeNumber} (range={new DatePeriod(start, end)}) custom results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return results;
    }

    /// <inheritdoc />
    public IList<Tuple<decimal, string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetConsolidatedWageTypeCustomResults(IList<decimal> wageTypeNumbers,
        DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null, bool noRetro = false)
    {
        if (wageTypeNumbers == null)
        {
            throw new ArgumentNullException(nameof(wageTypeNumbers));
        }
        foreach (var wageTypeNumber in wageTypeNumbers)
        {
            if (wageTypeNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(wageTypeNumber));
            }
        }

        var consolidatedResults = new List<Tuple<decimal, string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>>();
        if (EmployeeId == null)
        {
            throw new PayrollException("Missing employee on consolidated collector custom result request.");
        }

        var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
        if (periodStarts.Any() && EmployeeId != null)
        {
            var results = ResultProvider.GetConsolidatedWageTypeCustomResultsAsync(Settings.DbContext,
                new()
                {
                    TenantId = TenantId,
                    EmployeeId = EmployeeId.Value,
                    DivisionId = PayrunJob.DivisionId,
                    WageTypeNumbers = wageTypeNumbers,
                    PeriodStarts = periodStarts,
                    Forecast = forecast,
                    Tags = tags,
                    JobStatus = (PayrunJobStatus?)jobStatus,
                    EvaluationDate = EvaluationDate,
                    NoRetro = noRetro,
                    ExcludeParentJobId = ParentPayrunJob?.Id,
                }).GetAwaiter().GetResult().Select(x =>
                new Tuple<decimal, string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
                    x.WageTypeNumber, x.WageTypeName, x.Source, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();

            // collect results
            consolidatedResults.AddRange(results);
        }

#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Wage Type {wageTypeNumber} (moment={periodMoment}) consolidated custom results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return consolidatedResults;
    }

    /// <inheritdoc />
    public IList<decimal> GetRetroWageTypeResults(decimal wageTypeNumber,
        string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (wageTypeNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wageTypeNumber));
        }
        if (!EmployeeId.HasValue)
        {
            throw new PayrollException("Missing employee on retro wage type result request.");
        }

        var results = new List<decimal>();
        if (PayrunJob.RetroPayMode != RetroPayMode.None)
        {
            //  get retro results by the current job (current=parent)
            var retroResults = ResultProvider.GetWageTypeResultsAsync(Settings.DbContext,
                new()
                {
                    TenantId = TenantId,
                    EmployeeId = EmployeeId.Value,
                    DivisionId = PayrunJob.DivisionId,
                    WageTypeNumbers = [wageTypeNumber],
                    Forecast = forecast,
                    Tags = tags,
                    JobStatus = (PayrunJobStatus?)jobStatus,
                },
                parentPayrunJobId: PayrunJob.Id).GetAwaiter().GetResult().ToList();
            if (retroResults.Any())
            {
                // sort from oldest to newest
                retroResults.Sort((x, y) => DateTime.Compare(x.Start, y.Start));
                if (!EmployeeId.HasValue)
                {
                    throw new PayrollException("Missing employee on retro wage type result request.");
                }

                //  results period
                var periodStart = retroResults.First().Start.ToUtc();
                var periodEnd = retroResults.Last().End.ToUtc();
                var periodResults = ResultProvider.GetWageTypeResultsAsync(Settings.DbContext,
                    new()
                    {
                        TenantId = TenantId,
                        EmployeeId = EmployeeId.Value,
                        DivisionId = PayrunJob.DivisionId,
                        WageTypeNumbers = [wageTypeNumber],
                        Period = new(periodStart, periodEnd),
                        Forecast = forecast,
                        Tags = tags
                    }).GetAwaiter().GetResult().ToList();
                if (periodResults.Any())
                {
                    // group values by period
                    var resultsByPeriods = periodResults.GroupBy(x => x.Start);
                    foreach (var resultsByPeriod in resultsByPeriods)
                    {
                        var orderedResults = resultsByPeriod.OrderBy(x => x.Created).ToList();
                        if (orderedResults.Count > 1)
                        {
                            // calculate retro diff by subtracting the newest value with the previous value
                            var diff = orderedResults[^1].Value - orderedResults[^2].Value;
                            results.Add(diff);
                        }
                        else if (orderedResults.Count == 1 && resultsByPeriod.Key == periodStart)
                        {
                            // initial retro change without previous value
                            results.Add(orderedResults.First().Value);
                        }
                    }
#if SCRIPT_RESULT_REQUESTS
                        Log.Information($"Wage Type {wageTypeNumber} retro results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Get employee wage type results by date range.
    /// Serves from the pre-loaded YTD cache when the query matches the cache's range and numbers.
    /// </summary>
    private IList<WageTypeResult> GetWageTypeResultsInternal(IList<decimal> wageTypeNumbers, DateTime start, DateTime end,
        string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (wageTypeNumbers == null)
        {
            throw new ArgumentNullException(nameof(wageTypeNumbers));
        }
        foreach (var wageTypeNumber in wageTypeNumbers)
        {
            if (wageTypeNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(wageTypeNumber));
            }
        }
        if (!EmployeeId.HasValue)
        {
            throw new PayrollException("Missing employee on wage type result request.");
        }

        if (start >= end)
        {
            return new List<WageTypeResult>();
        }

        // serve from pre-loaded cycle cache when available and range matches
        var cycleCache = Settings.WageTypeYtdCache;
        if (cycleCache != null && cycleCache.CanServe(wageTypeNumbers, start, end))
        {
            return cycleCache.Get(wageTypeNumbers);
        }

        return ResultProvider.GetWageTypeResultsAsync(Settings.DbContext,
            new()
            {
                TenantId = TenantId,
                EmployeeId = EmployeeId.Value,
                DivisionId = PayrunJob.DivisionId,
                WageTypeNumbers = wageTypeNumbers,
                Period = new(start, end),
                Forecast = forecast,
                Tags = tags,
                JobStatus = (PayrunJobStatus?)jobStatus,
                EvaluationDate = EvaluationDate
            }).GetAwaiter().GetResult().ToList();
    }

    /// <summary>
    /// Get employee wage type custom results by date range
    /// </summary>
    private IList<WageTypeCustomResult> GetWageTypeCustomResultsInternal(IList<decimal> wageTypeNumbers, DateTime start, DateTime end,
        string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (wageTypeNumbers == null)
        {
            throw new ArgumentNullException(nameof(wageTypeNumbers));
        }
        foreach (var wageTypeNumber in wageTypeNumbers)
        {
            if (wageTypeNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(wageTypeNumber));
            }
        }
        if (!EmployeeId.HasValue)
        {
            throw new PayrollException("Missing employee on wage type custom result request.");
        }

        if (start >= end)
        {
            return new List<WageTypeCustomResult>();
        }
        return ResultProvider.GetWageTypeCustomResultsAsync(Settings.DbContext,
            new()
            {
                TenantId = TenantId,
                EmployeeId = EmployeeId.Value,
                DivisionId = PayrunJob.DivisionId,
                WageTypeNumbers = wageTypeNumbers,
                Period = new(start, end),
                Forecast = forecast,
                Tags = tags,
                JobStatus = (PayrunJobStatus?)jobStatus,
                EvaluationDate = EvaluationDate
            }).GetAwaiter().GetResult().ToList();
    }

    #endregion

    #region Collector

    /// <inheritdoc />
    public IList<Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetCollectorResults(
        IList<string> collectorNames, DateTime start, DateTime end, string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        var collectorResults = GetCollectorResultsInternal(collectorNames, start, end, forecast, jobStatus, tags);
        var results = collectorResults.Select(x => new Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
            x.CollectorName, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();
#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Collector {collectorName} (range={new DatePeriod(start, end)}) results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return results;
    }

    /// <inheritdoc />
    public IList<Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetConsolidatedCollectorResults(
        IList<string> collectorNames, DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null, bool noRetro = false)
    {
        if (EmployeeId == null)
        {
            throw new PayrollException("Missing employee on consolidated collector custom result request.");
        }

        // namespace
        if (!string.IsNullOrWhiteSpace(Namespace))
        {
            collectorNames = collectorNames.EnsureNamespace(Namespace);
        }

        // iterate from the start period until the job period
        var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
        var consolidatedResults = new List<Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>>();
        if (periodStarts.Any() && EmployeeId != null)
        {
            var results = ResultProvider.GetConsolidatedCollectorResultsAsync(Settings.DbContext,
                new()
                {
                    TenantId = TenantId,
                    EmployeeId = EmployeeId.Value,
                    DivisionId = PayrunJob.DivisionId,
                    CollectorNames = collectorNames,
                    PeriodStarts = periodStarts,
                    Forecast = forecast,
                    Tags = tags,
                    JobStatus = (PayrunJobStatus?)jobStatus,
                    EvaluationDate = EvaluationDate,
                    NoRetro = noRetro,
                    ExcludeParentJobId = ParentPayrunJob?.Id,
                }).GetAwaiter().GetResult().Select(x => new Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
                x.CollectorName, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();

            // collect results
            consolidatedResults.AddRange(results);
        }

#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Collector {collectorName} (moment={periodMoment}) consolidated results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return consolidatedResults;
    }

    /// <inheritdoc />
    public IList<Tuple<string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetCollectorCustomResults(
        IList<string> collectorNames, DateTime start, DateTime end, string forecast = null,
        int? jobStatus = null, IList<string> tags = null)
    {
        // namespace
        if (!string.IsNullOrWhiteSpace(Namespace))
        {
            collectorNames = collectorNames.EnsureNamespace(Namespace);
        }

        var collectorResults = GetCollectorCustomResultsInternal(collectorNames, start, end, forecast, jobStatus, tags);
        var results = collectorResults.Select(x => new Tuple<string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
            x.CollectorName, x.Source, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();
#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Collector {collectorName} (range={new DatePeriod(start, end)}) results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return results;
    }

    /// <inheritdoc />
    public IList<Tuple<string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>> GetConsolidatedCollectorCustomResults(IList<string> collectorNames,
        DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null, bool noRetro = false)
    {
        if (EmployeeId == null)
        {
            throw new PayrollException("Missing employee on consolidated collector custom result request.");
        }

        // namespace
        if (!string.IsNullOrWhiteSpace(Namespace))
        {
            collectorNames = collectorNames.EnsureNamespace(Namespace);
        }

        // iterate from the start period until the job period
        var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
        var consolidatedResults = new List<Tuple<string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>>();
        if (periodStarts.Any() && EmployeeId != null)
        {
            var results = ResultProvider.GetConsolidatedCollectorCustomResultsAsync(Settings.DbContext,
                new()
                {
                    TenantId = TenantId,
                    EmployeeId = EmployeeId.Value,
                    DivisionId = PayrunJob.DivisionId,
                    CollectorNames = collectorNames,
                    PeriodStarts = periodStarts,
                    Forecast = forecast,
                    Tags = tags,
                    JobStatus = (PayrunJobStatus?)jobStatus,
                    EvaluationDate = EvaluationDate,
                    NoRetro = noRetro,
                    ExcludeParentJobId = ParentPayrunJob?.Id,
                }).GetAwaiter().GetResult().Select(x =>
                new Tuple<string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
                    x.CollectorName, x.Source, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();

            // collect results
            consolidatedResults.AddRange(results);
        }

#if SCRIPT_RESULT_REQUESTS
            Log.Information($"Collector {collectorName} (moment={periodMoment}) consolidated custom results for period {CaseValueProvider.EvaluationPeriod}: {string.Join(',', results)}");
#endif
        return consolidatedResults;
    }

    /// <summary>
    /// Get employee collector results by date range
    /// </summary>
    private IList<CollectorResult> GetCollectorResultsInternal(IList<string> collectorNames, DateTime start, DateTime end,
        string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (!EmployeeId.HasValue)
        {
            throw new PayrollException("Missing employee on collector result request.");
        }

        if (start >= end)
        {
            return new List<CollectorResult>();
        }

        // namespace
        if (!string.IsNullOrWhiteSpace(Namespace))
        {
            collectorNames = collectorNames.EnsureNamespace(Namespace);
        }

        var results = ResultProvider.GetCollectorResultsAsync(Settings.DbContext,
            new()
            {
                TenantId = TenantId,
                EmployeeId = EmployeeId.Value,
                DivisionId = PayrunJob.DivisionId,
                CollectorNames = collectorNames,
                Period = new(start, end),
                Forecast = forecast,
                Tags = tags,
                JobStatus = (PayrunJobStatus?)jobStatus,
                EvaluationDate = EvaluationDate
            }).GetAwaiter().GetResult().ToList();
        return results;
    }

    /// <summary>
    /// Get employee collector custom results by date range
    /// </summary>
    private IList<CollectorCustomResult> GetCollectorCustomResultsInternal(IList<string> collectorNames, DateTime start, DateTime end,
        string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (!EmployeeId.HasValue)
        {
            throw new PayrollException("Missing employee on wage type custom result request.");
        }

        if (start >= end)
        {
            return new List<CollectorCustomResult>();
        }

        // namespace
        if (!string.IsNullOrWhiteSpace(Namespace))
        {
            collectorNames = collectorNames.EnsureNamespace(Namespace);
        }

        return ResultProvider.GetCollectorCustomResultsAsync(Settings.DbContext,
            new()
            {
                TenantId = TenantId,
                EmployeeId = EmployeeId.Value,
                DivisionId = PayrunJob.DivisionId,
                CollectorNames = collectorNames,
                Period = new(start, end),
                Forecast = forecast,
                Tags = tags,
                JobStatus = (PayrunJobStatus?)jobStatus,
                EvaluationDate = EvaluationDate
            }).GetAwaiter().GetResult().ToList();
    }

    /// <summary>Get consolidated period starts from a moment until the current period</summary>
    /// <param name="periodMoment">Moment within the starting period</param>
    /// <remarks>keep in sync with client scripting ConsolidatedResultCache</remarks>
    /// <returns>List of period start dates</returns>
    private List<DateTime> GetConsolidatedPeriodStarts(DateTime periodMoment)
    {
        var periodStarts = new List<DateTime>();
        var period = CaseValueProvider.PayrollCalculator.GetPayrunPeriod(periodMoment);
        // iterate from the start period until the job period
        while (period.Start < PayrunJob.PeriodStart)
        {
            periodStarts.Add(period.Start);
            // next period
            period = period.GetPayrollPeriod(period.Start, 1);
        }
        return periodStarts;
    }

    #endregion

}