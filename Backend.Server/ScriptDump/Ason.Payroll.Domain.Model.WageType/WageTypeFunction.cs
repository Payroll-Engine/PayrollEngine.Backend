/* WageTypeFunction */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Base class for wage type functions</summary>
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WageTypeFunction : PayrunFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    protected WageTypeFunction(object runtime) :
        base(runtime)
    {
        // wage type
        WageTypeNumber = Runtime.WageTypeNumber;
        WageTypeName = Runtime.WageTypeName;
        WageTypeDescription = Runtime.WageTypeDescription;

        // lookups
        Attribute = new(name => new PayrollValue(GetResultAttribute(name)), SetResultAttribute);
        AttributePayrollValue = new(name => new(GetResultAttribute(name)),
            (name, value) => SetResultAttribute(name, value.Value));
        WageType = new(wageTypeNumber => Runtime.GetWageTypeValue(wageTypeNumber));
        Collector = new(collectorName => Runtime.GetCollectorValue(collectorName));
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <param name="sourceFileName">The name of the source file</param>
    protected WageTypeFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    /// <summary>Get or set a wage type result attribute value</summary>
    public ScriptDictionary<string, object> Attribute { get; }

    /// <summary>Get or set a wage type result attribute <see cref="PayrollValue"/></summary>
    public ScriptDictionary<string, PayrollValue> AttributePayrollValue { get; }

    /// <summary>Get a wage type value by wage type number</summary>
    public ScriptDictionary<decimal, decimal> WageType { get; }

    /// <summary>Get a collector value by collector name</summary>
    public ScriptDictionary<string, decimal> Collector { get; }

    /// <summary>The wage type number</summary>
    public decimal WageTypeNumber { get; }

    /// <summary>The wage type name</summary>
    public string WageTypeName { get; }

    /// <summary>The wage type description</summary>
    public string WageTypeDescription { get; }

    /// <summary>The wage type collectors</summary>
    public string[] Collectors => Runtime.Collectors;

    /// <summary>The wage type collector groups</summary>
    public string[] CollectorGroups => Runtime.CollectorGroups;

    /// <summary>Reenable disabled collector for the current employee job</summary>
    /// <param name="collectorName">Name of the collector</param>
    public void EnableCollector(string collectorName) =>
        Runtime.EnableCollector(collectorName);

    /// <summary>Disable collector for the current employee job</summary>
    /// <param name="collectorName">Name of the collector</param>
    public void DisableCollector(string collectorName) =>
        Runtime.DisableCollector(collectorName);

    /// <summary>Get the wage type result tags</summary>
    /// <returns>The wage type result tags</returns>
    public List<string> GetResultTags() => Runtime.GetResultTags();

    /// <summary>Set the collector result tags</summary>
    /// <param name="tags">The result tags</param>
    public void SetResultTags(IEnumerable<string> tags) =>
        Runtime.SetResultTags(tags.ToList());

    /// <summary>Get wage result attribute</summary>
    /// <param name="name">The attribute name</param>
    public object GetResultAttribute(string name) =>
        Runtime.GetResultAttribute(name);

    /// <summary>Get wage result attribute typed value</summary>
    public T GetResultAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = GetResultAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>Sets the wage result attribute value</summary>
    /// <param name="name">The attribute name</param>
    /// <param name="value">The attribute value</param>
    public void SetResultAttribute(string name, object value) =>
        Runtime.SetResultAttribute(name, value);

    /// <summary>Get attribute value</summary>
    public object GetWageTypeAttribute(string attributeName) =>
        Runtime.GetWageTypeAttribute(attributeName);

    /// <summary>Get attribute typed value</summary>
    public T GetWageTypeAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = Runtime.GetWageTypeAttribute(attributeName);
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

    /// <summary>Get consolidated and current period employee wage type results by query</summary>
    /// <param name="query">The result query</param>
    /// <returns>Consolidated employee collector results</returns>
    public decimal GetWageTypeCurrentConsolidatedValue(WageTypeConsolidatedResultQuery query)
    {
        var value = GetConsolidatedWageTypeResults(query).Sum();
        foreach (var wageType in query.WageTypes)
        {
            value += WageType[wageType];

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

    /// <summary>Add wage type custom result from case field values, using the current period</summary>
    /// <param name="source">The value source</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The wage type custom result attributes</param>
    /// <param name="valueType">The result value type (numeric), default is the wage type value type</param>
    public void AddCustomResult(string source, IEnumerable<string> tags = null,
        Dictionary<string, object> attributes = null, ValueType? valueType = null) =>
        AddCustomResult(source, PeriodStart, PeriodEnd, tags, attributes, valueType);

    /// <summary>Add wage type custom result from case field values, using the current period</summary>
    /// <param name="source">The value source</param>
    /// <param name="value">The period value</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The wage type custom result attributes</param>
    /// <param name="valueType">The result value type (numeric), default is the wage type value type</param>
    public void AddCustomResult(string source, decimal value, IEnumerable<string> tags = null,
        Dictionary<string, object> attributes = null, ValueType? valueType = null) =>
        AddCustomResult(source, value, PeriodStart, PeriodEnd, tags, attributes, valueType);

    /// <summary>Add wage type custom result from case field values</summary>
    /// <param name="source">The value source</param>
    /// <param name="startDate">The moment within the start period</param>
    /// <param name="endDate">The moment within the end period</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The wage type custom result attributes</param>
    /// <param name="valueType">The result value type (numeric), default is the wage type value type</param>
    public void AddCustomResult(string source, DateTime startDate, DateTime endDate,
        IEnumerable<string> tags = null, Dictionary<string, object> attributes = null,
        ValueType? valueType = null)
    {
        var tagList = tags?.ToList();
        var caseValues = GetPeriodCaseValues(new DatePeriod(startDate, endDate), source);
        foreach (var caseValue in caseValues)
        {
            foreach (var periodValue in caseValue.Value.PeriodValues)
            {
                if (periodValue.Value is decimal decimalValue)
                {
                    var period = new DatePeriod(periodValue.Start, periodValue.End);
                    AddCustomResult(source, decimalValue, period.Start, period.End, tagList, attributes, valueType);
                }
            }
        }
    }

    /// <summary>Add a wage type custom result</summary>
    /// <param name="source">The value source</param>
    /// <param name="value">The period value</param>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <param name="tags">The result tags</param>
    /// <param name="attributes">The wage type custom result attributes</param>
    /// <param name="valueType">The result value type (numeric), default is the wage type value type</param>
    public void AddCustomResult(string source, decimal value, DateTime startDate,
        DateTime endDate, IEnumerable<string> tags = null, Dictionary<string, object> attributes = null,
        ValueType? valueType = null) =>
        Runtime.AddCustomResult(source, value, startDate, endDate, tags?.ToList(), attributes, (int?)valueType);

    #endregion

    #region Retro

    /// <summary>Schedule a retro payrun</summary>
    /// <param name="scheduleDate">The payrun schedule date, must be before the current period</param>
    /// <param name="resultTags">The result tags</param>
    public void ScheduleRetroPayrun(DateTime scheduleDate, IEnumerable<string> resultTags = null) =>
        Runtime.ScheduleRetroPayrun(scheduleDate, resultTags?.ToList());

    #endregion

}