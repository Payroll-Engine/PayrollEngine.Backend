﻿//#define SCRIPT_RESULT_REQUESTS
using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

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
    internal List<PayrunResult> PayrunResults { get; } = new();

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
            : default;

    /// <inheritdoc />
    public string Forecast => PayrunJob.Forecast;

    /// <inheritdoc />
    public string CycleName => PayrunJob.CycleName;

    /// <inheritdoc />
    public string PeriodName => PayrunJob.PeriodName;

    /// <inheritdoc />
    public object GetPayrunJobAttribute(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        return PayrunJob.Attributes?.GetValue<object>(attributeName);
    }

    /// <inheritdoc />
    public void SetPayrunJobAttribute(string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        PayrunJob.Attributes ??= new();
        PayrunJob.Attributes[attributeName] = value;
    }

    /// <inheritdoc />
    public bool RemovePayrunJobAttribute(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        return PayrunJob.Attributes != null && PayrunJob.Attributes.Remove(attributeName);
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
        return RuntimeValueProvider.PayrunValues.TryGetValue(key, out var value) ? value : null;
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
            if (RuntimeValueProvider.PayrunValues.ContainsKey(key))
            {
                RuntimeValueProvider.PayrunValues.Remove(key);
            }
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
        return RuntimeValueProvider.EmployeeValues.ContainsKey(employee) &&
               RuntimeValueProvider.EmployeeValues[employee].ContainsKey(key);
    }

    /// <inheritdoc />
    public string GetEmployeeRuntimeValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        var employee = Employee.Identifier;
        if (!RuntimeValueProvider.EmployeeValues.ContainsKey(employee))
        {
            return null;
        }
        return RuntimeValueProvider.EmployeeValues[employee].TryGetValue(key, out var value) ? value : null;
    }

    /// <inheritdoc />
    public void SetEmployeeRuntimeValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }

        var employee = Employee.Identifier;
        if (!RuntimeValueProvider.EmployeeValues.TryGetValue(employee, out var employeeValues))
        {
            employeeValues = new();
            RuntimeValueProvider.EmployeeValues[employee] = employeeValues;
        }
        if (value == null)
        {
            // remove value
            if (employeeValues.ContainsKey(key))
            {
                employeeValues.Remove(key);
            }
        }
        else
        {
            employeeValues[key] = value;
        }
    }

    #endregion

    #region Wage Type Results

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
        IList<decimal> wageTypeNumbers, DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (wageTypeNumbers == null)
        {
            throw new ArgumentNullException(nameof(wageTypeNumbers));
        }

        var consolidatedResults = new List<Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>>();
        if (EmployeeId != null)
        {
            var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
            if (periodStarts.Any() && EmployeeId != null)
            {
                var results = Task.Run(() => ResultProvider.GetConsolidatedWageTypeResultsAsync(Settings.DbContext,
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
                        EvaluationDate = EvaluationDate
                    })).Result.Select(x => new Tuple<decimal, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
                    x.WageTypeNumber, x.WageTypeName, new(x.Start, x.End), x.Value, x.Tags, x.Attributes)).ToList();

                // collect results
                consolidatedResults.AddRange(results);
            }
        }
        else
        {
            throw new PayrollException("Missing employee on consolidated collector result request");
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
        DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null)
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
            throw new PayrollException("Missing employee on consolidated collector custom result request");
        }

        var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
        if (periodStarts.Any() && EmployeeId != null)
        {
            var results = Task.Run(() => ResultProvider.GetConsolidatedWageTypeCustomResultsAsync(Settings.DbContext,
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
                    EvaluationDate = EvaluationDate
                })).Result.Select(x =>
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
            throw new PayrollException("Missing employee on retro wage type result request");
        }

        var results = new List<decimal>();
        if (PayrunJob.RetroPayMode != RetroPayMode.None)
        {
            //  get retro results by the current job (current=parent)
            var retroResults = Task.Run(() => ResultProvider.GetWageTypeResultsAsync(Settings.DbContext,
                new()
                {
                    TenantId = TenantId,
                    EmployeeId = EmployeeId.Value,
                    DivisionId = PayrunJob.DivisionId,
                    WageTypeNumbers = new[] { wageTypeNumber },
                    Forecast = forecast,
                    Tags = tags,
                    JobStatus = (PayrunJobStatus?)jobStatus,
                },
                parentPayrunJobId: PayrunJob.Id)).Result.ToList();
            if (retroResults.Any())
            {
                // sort from oldest to newest
                retroResults.Sort((x, y) => DateTime.Compare(x.Start, y.Start));
                if (!EmployeeId.HasValue)
                {
                    throw new PayrollException("Missing employee on retro wage type result request");
                }

                //  results period
                var periodStart = retroResults.First().Start.ToUtc();
                var periodEnd = retroResults.Last().End.ToUtc();
                var periodResults = Task.Run(() => ResultProvider.GetWageTypeResultsAsync(Settings.DbContext,
                    new()
                    {
                        TenantId = TenantId,
                        EmployeeId = EmployeeId.Value,
                        DivisionId = PayrunJob.DivisionId,
                        WageTypeNumbers = new[] { wageTypeNumber },
                        Period = new(periodStart, periodEnd),
                        Forecast = forecast,
                        Tags = tags
                    })).Result.ToList();
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
    /// Get employee wage type results by date range
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
            throw new PayrollException("Missing employee on wage type result request");
        }

        if (start >= end)
        {
            return new List<WageTypeResult>();
        }
        return Task.Run(() => ResultProvider.GetWageTypeResultsAsync(Settings.DbContext,
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
            })).Result.ToList();
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
            throw new PayrollException("Missing employee on wage type custom result request");
        }

        if (start >= end)
        {
            return new List<WageTypeCustomResult>();
        }
        return Task.Run(() => ResultProvider.GetWageTypeCustomResultsAsync(Settings.DbContext,
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
            })).Result.ToList();
    }

    #endregion

    #region Collector Results

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
        IList<string> collectorNames, DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (collectorNames == null)
        {
            throw new ArgumentNullException(nameof(collectorNames));
        }
        foreach (var collectorName in collectorNames)
        {
            if (string.IsNullOrWhiteSpace(collectorName))
            {
                throw new ArgumentException(nameof(collectorName));
            }
        }

        var consolidatedResults = new List<Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>>();
        if (EmployeeId == null)
        {
            throw new PayrollException("Missing employee on consolidated collector custom result request");
        }

        // iterate from the start period until the job period
        var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
        if (periodStarts.Any() && EmployeeId != null)
        {
            var results = Task.Run(() => ResultProvider.GetConsolidatedCollectorResultsAsync(Settings.DbContext,
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
                    EvaluationDate = EvaluationDate
                })).Result.Select(x => new Tuple<string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>(
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
        DateTime periodMoment, string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (collectorNames == null)
        {
            throw new ArgumentNullException(nameof(collectorNames));
        }
        foreach (var collectorName in collectorNames)
        {
            if (string.IsNullOrWhiteSpace(collectorName))
            {
                throw new ArgumentException(nameof(collectorName));
            }
        }

        var consolidatedResults = new List<Tuple<string, string, Tuple<DateTime, DateTime>, decimal, List<string>, Dictionary<string, object>>>();
        if (EmployeeId == null)
        {
            throw new PayrollException("Missing employee on consolidated collector custom result request");
        }

        // iterate from the start period until the job period
        var periodStarts = GetConsolidatedPeriodStarts(periodMoment);
        if (periodStarts.Any() && EmployeeId != null)
        {
            var results = Task.Run(() => ResultProvider.GetConsolidatedCollectorCustomResultsAsync(Settings.DbContext,
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
                    EvaluationDate = EvaluationDate
                })).Result.Select(x =>
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
        if (collectorNames == null)
        {
            throw new ArgumentNullException(nameof(collectorNames));
        }
        foreach (var collectorName in collectorNames)
        {
            if (string.IsNullOrWhiteSpace(collectorName))
            {
                throw new ArgumentException(nameof(collectorName));
            }
        }
        if (!EmployeeId.HasValue)
        {
            throw new PayrollException("Missing employee on collector result request");
        }

        if (start >= end)
        {
            return new List<CollectorResult>();
        }

        var results = Task.Run(() => ResultProvider.GetCollectorResultsAsync(Settings.DbContext,
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
            })).Result.ToList();
        return results;
    }

    /// <summary>
    /// Get employee collector custom results by date range
    /// </summary>
    private IList<CollectorCustomResult> GetCollectorCustomResultsInternal(IList<string> collectorNames, DateTime start, DateTime end,
        string forecast = null, int? jobStatus = null, IList<string> tags = null)
    {
        if (collectorNames == null)
        {
            throw new ArgumentNullException(nameof(collectorNames));
        }
        foreach (var collectorName in collectorNames)
        {
            if (string.IsNullOrWhiteSpace(collectorName))
            {
                throw new ArgumentException(nameof(collectorName));
            }
        }
        if (!EmployeeId.HasValue)
        {
            throw new PayrollException("Missing employee on wage type custom result request");
        }

        if (start >= end)
        {
            return new List<CollectorCustomResult>();
        }
        return Task.Run(() => ResultProvider.GetCollectorCustomResultsAsync(Settings.DbContext,
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
            })).Result.ToList();
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