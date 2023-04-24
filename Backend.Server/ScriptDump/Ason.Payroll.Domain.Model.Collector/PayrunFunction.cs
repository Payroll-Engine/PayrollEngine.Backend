/* PayrunFunction */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Base class for Payrun functions</summary>
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class PayrunFunction : PayrollFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    protected PayrunFunction(object runtime) :
        base(runtime)
    {
        // payrun
        PayrunId = Runtime.PayrunId;
        PayrunName = Runtime.PayrunName;

        // payrun job
        ExecutionPhase = (PayrunExecutionPhase)Runtime.ExecutionPhase;
        RetroPeriod = Runtime.RetroPeriod is not Tuple<DateTime, DateTime> retroPeriod ? null :
            new DatePeriod(retroPeriod.Item1, retroPeriod.Item2);
        Forecast = Runtime.Forecast;
        CycleName = Runtime.CycleName;
        PeriodName = Runtime.PeriodName;
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <param name="sourceFileName">The name of the source file</param>
    protected PayrunFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    #region Payrun

    /// <summary>The payrun id</summary>
    public int PayrunId { get; }

    /// <summary>The payrun name</summary>
    public string PayrunName { get; }

    #endregion

    #region PayrunJob

    /// <summary>The payrun execution phase</summary>
    public PayrunExecutionPhase ExecutionPhase { get; }

    /// <summary>The retro payrun period</summary>
    public DatePeriod RetroPeriod { get; }

    /// <summary>True for a retro payrun</summary>
    public bool IsRetroPayrun => RetroPeriod != null;

    /// <summary>True for a retro payrun within the current cycle</summary>
    public bool IsCycleRetroPayrun =>
        RetroPeriod != null && Cycle.IsWithin(RetroPeriod);

    /// <summary>True for a forecast payrun</summary>
    public string Forecast { get; }

    /// <summary>True for a forecast payrun</summary>
    public bool IsForecast => !string.IsNullOrWhiteSpace(Forecast);

    /// <summary>The cycle name</summary>
    public string CycleName { get; }

    /// <summary>The period name</summary>
    public string PeriodName { get; }

    /// <summary>Get payrun job attribute value</summary>
    public object GetPayrunJobAttribute(string attributeName) =>
        Runtime.GetPayrunJobAttribute(attributeName);

    /// <summary>Get employee attribute typed value</summary>
    public T GetPayrunJobAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = GetPayrunJobAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>Set payrun job attribute value</summary>
    /// <param name="attributeName">Name of the attribute</param>
    /// <param name="value">The attribute value</param>
    public void SetPayrunJobAttribute(string attributeName, object value) =>
        Runtime.SetPayrunJobAttribute(attributeName, value);

    /// <summary>Remove payrun job attribute</summary>
    /// <param name="attributeName">Name of the attribute</param>
    public bool RemovePayrunJobAttribute(string attributeName) =>
        Runtime.RemovePayrunJobAttribute(attributeName);

    #endregion

    #region Runtime values

    /// <summary>Test for existing payrun runtime value</summary>
    /// <param name="key">The value key</param>
    /// <returns>True if the runtime value exists</returns>
    public bool HasPayrunRuntimeValue(string key) =>
        Runtime.HasPayrunRuntimeValue(key);

    /// <summary>Get payrun runtime value</summary>
    /// <param name="key">The value key</param>
    /// <returns>The payrun runtime value</returns>
    public string GetPayrunRuntimeValue(string key) =>
        Runtime.GetPayrunRuntimeValue(key);

    /// <summary>Get payrun runtime value as deserialized type</summary>
    /// <param name="key">The value key</param>
    /// <returns>The payrun runtime value</returns>
    public T GetPayrunRuntimeValue<T>(string key)
    {
        var value = GetPayrunRuntimeValue(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>Set payrun runtime value</summary>
    /// <param name="key">The value key</param>
    /// <param name="value">The payrun runtime value, use null to remove the runtime value</param>
    public void SetPayrunRuntimeValue(string key, string value) =>
        Runtime.SetPayrunRuntimeValue(key, value);

    /// <summary>Set payrun runtime value as serialized type</summary>
    /// <param name="key">The value key</param>
    /// <param name="value">The payrun runtime value</param>
    public void SetPayrunRuntimeValue<T>(string key, T value) =>
        SetPayrunRuntimeValue(key, JsonSerializer.Serialize(value));

    /// <summary>Test for existing employee runtime value</summary>
    /// <param name="key">The value key</param>
    /// <returns>True if the runtime value exists</returns>
    public bool HasEmployeeRuntimeValue(string key) =>
        Runtime.HasEmployeeRuntimeValue(key);

    /// <summary>Get employee runtime value</summary>
    /// <param name="key">The value key</param>
    /// <returns>The employee runtime value</returns>
    public string GetEmployeeRuntimeValue(string key) =>
        Runtime.GetEmployeeRuntimeValue(key);

    /// <summary>Get employee runtime value as deserialized type</summary>
    /// <param name="key">The value key</param>
    /// <returns>The employee runtime value</returns>
    public T GetEmployeeRuntimeValue<T>(string key)
    {
        var value = GetEmployeeRuntimeValue(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>Set employee runtime value</summary>
    /// <param name="key">The value key</param>
    /// <param name="value">The employee runtime value, use null to remove the runtime value</param>
    public void SetEmployeeRuntimeValue(string key, string value) =>
        Runtime.SetEmployeeRuntimeValue(key, value);

    /// <summary>Set employee runtime value as serialized type</summary>
    /// <param name="key">The value key</param>
    /// <param name="value">The employee runtime value</param>
    public void SetEmployeeRuntimeValue<T>(string key, T value) =>
        SetEmployeeRuntimeValue(key, JsonSerializer.Serialize(value));

    #endregion

    #region Wage Type Results

    /// <summary>Get employee wage types results by query</summary>
    /// <param name="query">The cycle query</param>
    /// <returns>Employee wage type cycle results</returns>
    public IList<WageTypeResult> GetWageTypeCycleResults(WageTypeCycleResultQuery query)
    {
        var period = GetCycle(query.CycleCount * -1);
        return GetWageTypeResults(query.WageTypes, period.Start, period.End, query.Forecast, query.JobStatus, query.Tags);
    }

    /// <summary>Get employee wage types results by payroll periods</summary>
    /// <param name="query">The period query</param>
    /// <returns>Employee wage type period results</returns>
    public IList<WageTypeResult> GetPeriodWageTypeResults(WageTypePeriodResultQuery query)
    {
        var period = GetPeriod(query.PeriodCount * -1);
        return GetWageTypeResults(query.WageTypes, period.Start, period.End, query.Forecast, query.JobStatus, query.Tags);
    }

    /// <summary>Get employee wage types results by query</summary>
    /// <param name="query">The range query</param>
    /// <returns>Employee wage type range results</returns>
    public IList<WageTypeResult> GetWageTypeResults(WageTypeRangeResultQuery query) =>
        GetWageTypeResults(query.WageTypes, query.Start, query.End, query.Forecast, query.JobStatus, query.Tags);

    /// <summary>Get employee wage type results by date range</summary>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <param name="start">The range start date</param>
    /// <param name="end">The range end date</param>
    /// <param name="jobStatus">The job status</param>
    /// <param name="forecast">The forecast</param>
    /// <param name="tags">The result tags</param>
    /// <returns>Employee wage type range results</returns>
    public IList<WageTypeResult> GetWageTypeResults(IEnumerable<decimal> wageTypeNumbers, DateTime start, DateTime end,
        string forecast = null, PayrunJobStatus? jobStatus = null, IEnumerable<string> tags = null) =>
        TupleExtensions.TupleToWageTypeResults(Runtime.GetWageTypeResults(wageTypeNumbers.ToList(), start, end,
            forecast, (int?)jobStatus, tags?.ToList()));

    /// <summary>Get consolidated employee wage type results by query</summary>
    /// <param name="query">The result query</param>
    /// <returns>Consolidated employee wage type results</returns>
    public IList<WageTypeResult> GetConsolidatedWageTypeResults(WageTypeConsolidatedResultQuery query) =>
        TupleExtensions.TupleToWageTypeResults(Runtime.GetConsolidatedWageTypeResults(query.WageTypes, query.PeriodMoment,
            query.Forecast, (int?)query.JobStatus, query.Tags));

    /// <summary>Get employee wage type custom results by query</summary>
    /// <param name="query">The result query</param>
    /// <returns>Consolidated employee wage type custom results</returns>
    public IList<WageTypeCustomResult> GetWageTypeCustomResults(WageTypeRangeResultQuery query) =>
        TupleExtensions.TupleToWageTypeCustomResults(Runtime.GetWageTypeCustomResults(query.WageTypes, query.Start,
            query.End, query.Forecast, (int?)query.JobStatus, query.Tags));

    /// <summary>Get consolidated employee wage type custom results by query</summary>
    /// <param name="query">The result query</param>
    /// <returns>Consolidated employee wage type custom results</returns>
    public IList<WageTypeCustomResult> GetConsolidatedWageTypeCustomResults(WageTypeConsolidatedResultQuery query) =>
        TupleExtensions.TupleToWageTypeCustomResults(Runtime.GetConsolidatedWageTypeCustomResults(query.WageTypes, query.PeriodMoment,
            query.Forecast, (int?)query.JobStatus, query.Tags));

    /// <summary>Get retro employee retro wage type results by periods</summary>
    /// <param name="query">The query</param>
    /// <returns>Retro employee retro wage type values</returns>
    public IList<decimal> GetWageTypeRetroResults(WageTypeResultQuery query) =>
        Runtime.GetRetroWageTypeResults(query.WageTypes[0], query.Forecast, (int?)query.JobStatus, query.Tags);

    /// <summary>Get summary of retro wage type results</summary>
    /// <param name="wageTypeNumber">The wage type number</param>
    /// <returns>Retro employee wage type value (difference)</returns>
    public decimal GetWageTypeRetroResultSum(decimal wageTypeNumber) =>
        GetWageTypeRetroResults(new(wageTypeNumber)).DefaultIfEmpty().Sum();

    #endregion

    #region Collector Results

    /// <summary>Get employee collector results by query</summary>
    /// <param name="query">The cycle query</param>
    /// <returns>Employee collector cycle results</returns>
    public IList<CollectorResult> GetCollectorCycleResults(CollectorCycleResultQuery query)
    {
        var period = GetCycle(query.CycleCount * -1);
        return GetCollectorResults(query.Collectors, period.Start, period.End, query.Forecast, query.JobStatus, query.Tags);
    }

    /// <summary>Get employee collector results by query</summary>
    /// <param name="query">The period query</param>
    /// <returns>Employee collector period results</returns>
    public IList<CollectorResult> GetCollectorPeriodResults(CollectorPeriodResultQuery query)
    {
        var period = GetPeriod(query.PeriodCount * -1);
        return GetCollectorResults(query.Collectors, period.Start, period.End, query.Forecast, query.JobStatus, query.Tags);
    }

    /// <summary>Get employee collectors results by custom date range</summary>
    /// <param name="query">The range query</param>
    /// <returns>Employee collector range results</returns>
    public IList<CollectorResult> GetCollectorResults(CollectorRangeResultQuery query) =>
        GetCollectorResults(query.Collectors, query.Start, query.End, query.Forecast, query.JobStatus, query.Tags);

    /// <summary>Get employee collectors results by custom date range</summary>
    /// <param name="collectorNames">The collector names</param>
    /// <param name="start">The range start date</param>
    /// <param name="end">The range end date</param>
    /// <param name="forecast">The forecast</param>
    /// <param name="jobStatus">The job status</param>
    /// <param name="tags">The result tags</param>
    /// <returns>Employee collector range results</returns>
    public IList<CollectorResult> GetCollectorResults(IEnumerable<string> collectorNames, DateTime start, DateTime end,
        string forecast = null, PayrunJobStatus? jobStatus = null, IEnumerable<string> tags = null) =>
        TupleExtensions.TupleToCollectorResults(Runtime.GetCollectorResults(collectorNames.ToList(),
            start, end, forecast, (int?)jobStatus, tags?.ToList()));

    /// <summary>Get consolidated employee collectors results by query</summary>
    /// <param name="query">The result query</param>
    /// <returns>Consolidated employee collector results</returns>
    public IList<CollectorResult> GetConsolidatedCollectorResults(CollectorConsolidatedResultQuery query) =>
        TupleExtensions.TupleToCollectorResults(Runtime.GetConsolidatedCollectorResults(query.Collectors,
            query.PeriodMoment, query.Forecast, (int?)query.JobStatus, query.Tags));

    /// <summary>Get consolidated employee collector custom results by query</summary>
    /// <param name="query">The result query</param>
    /// <returns>Consolidated employee collector custom results</returns>
    public IList<CollectorCustomResult> GetConsolidatedCollectorCustomResults(CollectorConsolidatedResultQuery query) =>
        TupleExtensions.TupleToCollectorCustomResults(Runtime.GetConsolidatedCollectorCustomResults(query.Collectors,
            query.PeriodMoment, query.Forecast, (int?)query.JobStatus, query.Tags));

    #endregion

    #region Webhooks

    /// <summary>Invoke payrun webhook</summary>
    /// <param name="requestOperation">The request operation</param>
    /// <param name="requestMessage">The webhook request message</param>
    /// <returns>The webhook response object</returns>
    public T InvokeWebhook<T>(string requestOperation, object requestMessage = null)
    {
        if (string.IsNullOrWhiteSpace(requestOperation))
        {
            throw new ArgumentException(nameof(requestOperation));
        }

        // webhook request
        var jsonRequest = requestMessage != null ? JsonSerializer.Serialize(requestMessage) : null;
        var jsonResponse = Runtime.InvokeWebhook(requestOperation, jsonRequest);
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            return default;
        }
        var response = JsonSerializer.Deserialize<T>(jsonResponse);
        return response;
    }

    #endregion

}