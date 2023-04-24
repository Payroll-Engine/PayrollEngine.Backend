/* CollectorFunction */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Base class for collector functions</summary>
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CollectorFunction : PayrunFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    protected CollectorFunction(object runtime) :
        base(runtime)
    {
        // collector
        CollectorName = Runtime.CollectorName;
        CollectorGroups = Runtime.CollectorGroups;
        CollectType = Runtime.CollectType;
        CollectorThreshold = Runtime.CollectorThreshold;
        CollectorMinResult = Runtime.CollectorMinResult;
        CollectorMaxResult = Runtime.CollectorMaxResult;
        CollectorResult = Runtime.CollectorResult;
        CollectorCount = Runtime.CollectorCount;
        CollectorSum = Runtime.CollectorSum;
        CollectorMin = Runtime.CollectorMin;
        CollectorMax = Runtime.CollectorMax;
        CollectorAverage = Runtime.CollectorAverage;

        // lookups
        Collector = new(collectorName => Runtime.GetCollectorValue(collectorName));
        ResultAttribute = new(name => new PayrollValue(Runtime.GetResultAttribute(name)),
            (name, value) => Runtime.SetResultAttribute(name, value));
        ResultAttributePayrollValue = new(name => new PayrollValue(Runtime.GetResultAttribute(name)),
            (name, value) => Runtime.SetResultAttribute(name, value.Value));
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <param name="sourceFileName">The name of the source file</param>
    protected CollectorFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    /// <summary>Get a collector value by collector name</summary>
    public ScriptDictionary<string, decimal> Collector { get; }

    /// <summary>Get or set a wage type result attribute value</summary>
    public ScriptDictionary<string, object> ResultAttribute { get; }

    /// <summary>Get or set a wage type result attribute <see cref="PayrollValue"/></summary>
    public ScriptDictionary<string, PayrollValue> ResultAttributePayrollValue { get; }

    /// <summary>The collector name</summary>
    public string CollectorName { get; }

    /// <summary>The collector groups</summary>
    public string[] CollectorGroups { get; }

    /// <summary>The collect type</summary>
    public string CollectType { get; }

    /// <summary>The threshold value</summary>
    public decimal? CollectorThreshold { get; }

    /// <summary>The minimum allowed value</summary>
    public decimal? CollectorMinResult { get; }

    /// <summary>The maximum allowed value</summary>
    public decimal? CollectorMaxResult { get; }

    /// <summary>The current collector result</summary>
    public decimal CollectorResult { get; }

    /// <summary>Collected values count</summary>
    public decimal CollectorCount { get; }

    /// <summary>The summary of the collected value</summary>
    public decimal CollectorSum { get; }

    /// <summary>The minimum collected value</summary>
    public decimal CollectorMin { get; }

    /// <summary>The maximum collected value</summary>
    public decimal CollectorMax { get; }

    /// <summary>The average of the collected value</summary>
    public decimal CollectorAverage { get; }

    /// <summary>Resets the collector to his initial state</summary>
    public void Reset() => Runtime.Reset();

    /// <summary>Get the collector result tags</summary>
    /// <returns>The collector result tags</returns>
    public List<string> GetResultTags() => Runtime.GetResultTags();

    /// <summary>Set the collector result tags</summary>
    /// <param name="tags">The result tags</param>
    public void SetResultTags(IEnumerable<string> tags) =>
        Runtime.SetResultTags(tags.ToList());

    /// <summary>Get attribute value</summary>
    /// <param name="attributeName">The attribute name</param>
    public object GetCollectorAttribute(string attributeName) =>
        Runtime.GetCollectorAttribute(attributeName);

    /// <summary>Get attribute typed value</summary>
    public T GetCollectorAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = Runtime.GetCollectorAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>Get consolidated and current period employee collector results by query</summary>
    /// <param name="query">The result query</param>
    /// <returns>Consolidated employee collector results</returns>
    public decimal GetCollectorCurrentConsolidatedValue(CollectorConsolidatedResultQuery query)
    {
        var value = GetConsolidatedCollectorResults(query).Sum();
        foreach (var collector in query.Collectors)
        {
            value += Collector[collector];
        }
        return value;
    }

    #region Payrun Results

    /// <summary>Add payrun result using the current period</summary>
    /// <param name="name">The result name</param>
    /// <param name="value">The result value</param>
    /// <param name="valueType">The result value type</param>
    /// <param name="source">The result source</param>
    /// <param name="slot">The result slot</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The wage type custom result attributes</param>
    public void AddPayrunResult(string name, object value, ValueType? valueType = null, string source = null,
        string slot = null, IEnumerable<string> tags = null, Dictionary<string, object> attributes = null) =>
        AddPayrunResult(name, value, PeriodStart, PeriodEnd, valueType, source, slot, tags, attributes);

    /// <summary>Add payrun result</summary>
    /// <param name="name">The result name</param>
    /// <param name="value">The result value</param>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <param name="valueType">The result value type</param>
    /// <param name="source">The result source</param>
    /// <param name="slot">The result slot</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The wage type custom result attributes</param>
    public void AddPayrunResult(string name, object value, DateTime startDate, DateTime endDate,
        ValueType? valueType = null, string source = null, string slot = null,
        IEnumerable<string> tags = null, Dictionary<string, object> attributes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        source ??= GetType().Name;
        var json = JsonSerializer.Serialize(value);
        valueType ??= value.GetValueType();
        Runtime.AddPayrunResult(source, name, json, (int)valueType.Value, startDate, endDate, slot, tags?.ToList(), attributes);
    }

    #endregion

    #region Custom Results

    /// <summary>Adds a custom collector result, using the current period</summary>
    /// <param name="source">The value source</param>
    /// <param name="value">The period value</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The collector custom result attributes</param>
    /// <param name="valueType">The result value type (numeric), default is the collector value type</param>
    public void AddCustomResult(string source, decimal value, IEnumerable<string> tags = null,
        Dictionary<string, object> attributes = null, ValueType? valueType = null) =>
        AddCustomResult(source, value, PeriodStart, PeriodEnd, tags, attributes, valueType);

    /// <summary>Adds a custom collector result</summary>
    /// <param name="source">The value source</param>
    /// <param name="value">The period value</param>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The collector custom result attributes</param>
    /// <param name="valueType">The result value type (numeric), default is the collector value type</param>
    public void AddCustomResult(string source, decimal value, DateTime startDate, DateTime endDate,
        IEnumerable<string> tags = null, Dictionary<string, object> attributes = null,
        ValueType? valueType = null) =>
        Runtime.AddCustomResult(source, value, startDate, endDate, tags?.ToList(), attributes, (int?)valueType);

    #endregion

    /// <summary>Schedule a retro payrun</summary>
    /// <param name="scheduleDate">The payrun schedule date, must be before the current period</param>
    /// <param name="resultTags">The result tags</param>
    public void ScheduleRetroPayrun(DateTime scheduleDate, IEnumerable<string> resultTags = null) =>
        Runtime.ScheduleRetroPayrun(scheduleDate, resultTags?.ToList());
}