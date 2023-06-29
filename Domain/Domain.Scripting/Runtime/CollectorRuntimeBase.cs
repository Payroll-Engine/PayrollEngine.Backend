using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// runtime for a collector function
/// </summary>
public abstract class CollectorRuntimeBase : PayrunRuntimeBase, ICollectorRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected new CollectorRuntimeSettings Settings => base.Settings as CollectorRuntimeSettings;

    /// <inheritdoc />
    public string CollectorName => Collector.Name;

    /// <inheritdoc />
    public string[] CollectorGroups => Collector.CollectorGroups?.ToArray();

    /// <inheritdoc />
    public string CollectType => Enum.GetName(typeof(CollectType), Collector.CollectType);

    /// <inheritdoc />
    public decimal? CollectorThreshold => Collector.Threshold;

    /// <inheritdoc />
    public decimal? CollectorMinResult => Collector.MinResult;

    /// <inheritdoc />
    public decimal? CollectorMaxResult => Collector.MaxResult;

    /// <inheritdoc />
    public decimal CollectorResult => Collector.Result;

    /// <inheritdoc />
    public decimal CollectorCount => Collector.Count;

    /// <inheritdoc />
    public decimal CollectorSum => Collector.Sum;

    /// <inheritdoc />
    public decimal CollectorMin => Collector.Min;

    /// <inheritdoc />
    public decimal CollectorMax => Collector.Max;

    /// <inheritdoc />
    public decimal CollectorAverage => Collector.Average;

    /// <inheritdoc />
    public List<string> GetResultTags() => CurrentCollectorResult.Tags;

    /// <inheritdoc />
    public void SetResultTags(List<string> tags) =>
        CurrentCollectorResult.Tags = tags;

    /// <inheritdoc />
    public object GetCollectorAttribute(string attributeName) =>
        Collector.Attributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    protected CollectorRuntimeBase(CollectorRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwner => CollectorName;

    /// <inheritdoc />
    public void Reset() => Collector.Reset();

    #region Internal

    /// <summary>The collector</summary>
    protected Collector Collector => Settings.Collector;

    /// <summary>The current wage type and collector results</summary>
    protected PayrollResultSet CurrentPayrollResult => Settings.CurrentPayrollResult;

    /// <summary>Result attributes</summary>
    private CollectorResultSet CurrentCollectorResult => Settings.CurrentCollectorResult;

    /// <summary>The scheduled retro payrun jobs</summary>
    internal List<RetroPayrunJob> RetroJobs { get; } = new();

    #endregion

    #region Collector Values

    /// <inheritdoc />
    public decimal GetCollectorValue(string collectorName)
    {
        if (string.IsNullOrWhiteSpace(collectorName))
        {
            throw new ArgumentException(nameof(collectorName));
        }

        var collectorResult =
            CurrentPayrollResult.CollectorResults.FirstOrDefault(cr => string.Equals(cr.CollectorName, collectorName));
        return collectorResult?.Value ?? default;
    }

    #endregion

    #region Result Attributes

    /// <inheritdoc />
    public object GetResultAttribute(string name) =>
        CurrentCollectorResult.Attributes[name];

    /// <inheritdoc />
    public void SetResultAttribute(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        // remove attribute
        if (value == null)
        {
            if (CurrentCollectorResult.Attributes.ContainsKey(name))
            {
                CurrentCollectorResult.Attributes.Remove(name);
            }
        }
        else
        {
            // set or update attribute
            CurrentCollectorResult.Attributes[name] = value;
        }
    }

    #endregion

    #region Results

    /// <inheritdoc />
    public void AddPayrunResult(string source, string name, string value, int valueType,
        DateTime startDate, DateTime endDate, string slot, List<string> tags, Dictionary<string, object> attributes)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(nameof(source));
        }
        if (startDate >= endDate)
        {
            throw new ArgumentException($"Invalid start date {startDate} on end {endDate}");
        }

        // ensure attributes collection
        attributes ??= new();

        // value type
        if (!Enum.IsDefined(typeof(ValueType), valueType))
        {
            throw new ArgumentException($"Unknown value type: {valueType}");
        }

        // result
        var result = new PayrunResult
        {
            Source = source,
            Name = name,
            // currently no support for localized custom payrun results
            Slot = slot,
            ValueType = (ValueType)valueType,
            Value = value,
            NumericValue = ValueConvert.ToNumber(value, (ValueType)valueType),
            Start = startDate,
            End = endDate,
            Tags = tags,
            Attributes = attributes
        };
        PayrunResults.Add(result);
    }

    /// <inheritdoc />
    public void AddCustomResult(string source, decimal value, DateTime startDate, DateTime endDate,
        List<string> tags, Dictionary<string, object> attributes, int? valueType)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(nameof(source));
        }
        if (startDate >= endDate)
        {
            throw new ArgumentException($"Invalid start date {startDate} on end {endDate}");
        }

        // ensure attributes collection
        attributes ??= new();

        // result value type
        var collectorValueType = Collector.ValueType;
        if (valueType.HasValue && Enum.IsDefined(typeof(ValueType), valueType.Value))
        {
            collectorValueType = (ValueType)valueType.Value;
        }
        if (!collectorValueType.IsNumber())
        {
            throw new ScriptException($"Value type for custom result must be numeric: {collectorValueType}");
        }

        // result
        var customResult = new Model.CollectorCustomResult
        {
            CollectorName = CollectorName,
            // currently no support for localized custom collector results
            Source = source,
            Value = value,
            ValueType = collectorValueType,
            Start = startDate,
            End = endDate,
            Tags = tags,
            Attributes = attributes
        };
        CurrentCollectorResult.CustomResults.Add(customResult);
    }

    #endregion

    #region Retro

    /// <inheritdoc />
    public void ScheduleRetroPayrun(DateTime scheduleDate, List<string> resultTags)
    {
        if (scheduleDate >= EvaluationPeriod.Start)
        {
            throw new ArgumentOutOfRangeException(nameof(scheduleDate), $"Retro date {scheduleDate} must be before the evaluation period {EvaluationPeriod.Start}");
        }
        RetroJobs.Add(new() { ScheduleDate = scheduleDate, ResultTags = resultTags });
    }

    #endregion

}