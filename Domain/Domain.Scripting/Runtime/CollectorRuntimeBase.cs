using System;
using System.Linq;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// runtime for a collector function
/// </summary>
public abstract class CollectorRuntimeBase : PayrunRuntimeBase, ICollectorRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    private new CollectorRuntimeSettings Settings => base.Settings as CollectorRuntimeSettings;

    /// <inheritdoc />
    public string CollectorName => Collector.Name;

    /// <inheritdoc />
    public string[] CollectorGroups => Collector.CollectorGroups?.ToArray();

    /// <inheritdoc />
    public string CollectMode => Enum.GetName(typeof(CollectMode), Collector.CollectMode);

    /// <inheritdoc />
    public bool Negated => Collector.Negated;

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
    public decimal CollectorSummary => Collector.Summary;

    /// <inheritdoc />
    public decimal CollectorMinimum => Collector.Minimum;

    /// <inheritdoc />
    public decimal CollectorMaximum => Collector.Maximum;

    /// <inheritdoc />
    public decimal CollectorAverage => Collector.Average;

    /// <inheritdoc />
    public decimal CollectorRange => Collector.Range;

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

    #region Culture and Calendar

    /// <inheritdoc />
    public override string GetDerivedCulture(int divisionId, int employeeId) =>
        Collector.Culture ?? base.GetDerivedCulture(divisionId, employeeId);

    #endregion

    #region Internal

    /// <summary>The collector</summary>
    protected Collector Collector => Settings.Collector;

    /// <summary>The current wage type and collector results</summary>
    private PayrollResultSet CurrentPayrollResult => Settings.CurrentPayrollResult;

    /// <summary>Result attributes</summary>
    private CollectorResultSet CurrentCollectorResult => Settings.CurrentCollectorResult;

    /// <summary>The scheduled retro payrun jobs</summary>
    internal List<RetroPayrunJob> RetroJobs { get; } = [];

    #endregion

    #region Collector Values

    /// <inheritdoc />
    public decimal GetCollectorValue(string collectorName)
    {
        if (string.IsNullOrWhiteSpace(collectorName))
        {
            throw new ArgumentException(nameof(collectorName));
        }

        // namespace
        collectorName = collectorName.EnsureNamespace(Namespace);

        var collectorResult =
            CurrentPayrollResult.CollectorResults.FirstOrDefault(cr => string.Equals(cr.CollectorName, collectorName));
        return collectorResult?.Value ?? 0;
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
            CurrentCollectorResult.Attributes.Remove(name);
        }
        else
        {
            // set or update attribute
            CurrentCollectorResult.Attributes[name] = value;
        }
    }

    #endregion

    #region Custom Results

    /// <inheritdoc />
    public void AddCustomResult(string source, decimal value, DateTime startDate, DateTime endDate,
        List<string> tags, Dictionary<string, object> attributes, int? valueType, string culture)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException(nameof(source));
        }
        if (startDate >= endDate)
        {
            throw new ArgumentException($"Invalid start date {startDate} on end {endDate}.");
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
            throw new ScriptException($"Value type for custom result must be numeric: {collectorValueType}.");
        }

        // culture
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = GetDerivedCulture(DivisionId, Employee.Id);
        }

        // result
        var customResult = new Model.CollectorCustomResult
        {
            CollectorName = CollectorName,
            // currently no support for localized custom collector results
            Source = source,
            Value = value,
            ValueType = collectorValueType,
            Culture = culture,
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
            throw new ArgumentOutOfRangeException(nameof(scheduleDate), $"Retro date {scheduleDate} must be before the evaluation period {EvaluationPeriod.Start}.");
        }
        RetroJobs.Add(new() { ScheduleDate = scheduleDate, ResultTags = resultTags });
    }

    #endregion

}