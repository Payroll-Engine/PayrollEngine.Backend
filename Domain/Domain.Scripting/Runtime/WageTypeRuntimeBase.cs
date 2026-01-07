using System;
using System.Linq;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;
using WageTypeCustomResult = PayrollEngine.Domain.Model.WageTypeCustomResult;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// runtime for a wage type function
/// </summary>
public abstract class WageTypeRuntimeBase : PayrunRuntimeBase, IWageTypeRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected new WageTypeRuntimeSettings Settings => base.Settings as WageTypeRuntimeSettings;

    /// <inheritdoc />
    public decimal WageTypeNumber => WageType.WageTypeNumber;

    /// <inheritdoc />
    public string WageTypeName => WageType.Name;

    /// <inheritdoc />
    public string WageTypeDescription => WageType.Description;

    /// <inheritdoc />
    public string WageTypeCalendar => WageType.Calendar;

    /// <inheritdoc />
    public string[] Collectors => WageType.Collectors?.ToArray();

    /// <inheritdoc />
    public string[] CollectorGroups => WageType.CollectorGroups?.ToArray();

    #region Culture and Calendar

    /// <inheritdoc />
    public override string GetDerivedCulture(int divisionId, int employeeId) =>
        WageType.Culture ?? base.GetDerivedCulture(divisionId, employeeId);

    /// <inheritdoc />
    public override string GetDerivedCalendar(int divisionId, int employeeId) =>
        WageTypeCalendar ?? base.GetDerivedCalendar(divisionId, employeeId);

    #endregion

    #region Internal

    /// <summary>The wage type</summary>
    protected WageType WageType => Settings.WageType;

    /// <summary>The wage type attributes</summary>
    private Dictionary<string, object> WageTypeAttributes => Settings.WageTypeAttributes;

    /// <summary>The current wage type and collector results</summary>
    private PayrollResultSet CurrentPayrollResult => Settings.CurrentPayrollResult;

    /// <summary>The current wage type result</summary>
    private WageTypeResultSet CurrentWageTypeResult => Settings.CurrentWageTypeResult;

    /// <summary>The scheduled retro payrun jobs</summary>
    internal List<RetroPayrunJob> RetroJobs { get; } = [];

    #endregion

    #region Result Attributes

    /// <inheritdoc />
    public object GetResultAttribute(string name) =>
        CurrentWageTypeResult.Attributes[name];

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
            CurrentWageTypeResult.Attributes.Remove(name);
        }
        else
        {
            // set or update attribute
            CurrentWageTypeResult.Attributes[name] = value;
        }
    }

    #endregion

    /// <inheritdoc />
    public List<string> GetResultTags() => CurrentWageTypeResult.Tags;

    /// <inheritdoc />
    public void SetResultTags(List<string> tags) =>
        CurrentWageTypeResult.Tags = tags;

    /// <inheritdoc />
    public object GetWageTypeAttribute(string attributeName) =>
        WageTypeAttributes?.GetValue<object>(attributeName);

    /// <inheritdoc />
    protected WageTypeRuntimeBase(WageTypeRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwner => $"WT {WageTypeNumber:0.####}";

    /// <inheritdoc />
    public decimal GetWageType(decimal wageTypeNumber)
    {
        if (wageTypeNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wageTypeNumber));
        }

        var wageTypeResult =
            CurrentPayrollResult.WageTypeResults.FirstOrDefault(wtr => wtr.WageTypeNumber == wageTypeNumber);
        return wageTypeResult?.Value ?? 0;
    }

    /// <inheritdoc />
    public decimal GetWageType(string wageTypeName)
    {
        if (string.IsNullOrWhiteSpace(wageTypeName))
        {
            throw new ArgumentException(nameof(wageTypeName));
        }

        // namespace
        wageTypeName = wageTypeName.EnsureNamespace(Namespace);

        var wageTypeResult =
            CurrentPayrollResult.WageTypeResults.FirstOrDefault(wtr => string.Equals(wtr.WageTypeName, wageTypeName));
        return wageTypeResult?.Value ?? 0;
    }

    /// <inheritdoc />
    public decimal GetCollector(string collectorName)
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

    /// <inheritdoc />
    public void EnableCollector(string collectorName)
    {
        if (string.IsNullOrWhiteSpace(collectorName))
        {
            throw new ArgumentException(nameof(collectorName));
        }

        // namespace
        collectorName = collectorName.EnsureNamespace(Namespace);

        if (Settings.DisabledCollectors.Contains(collectorName))
        {
            Settings.DisabledCollectors.Remove(collectorName);
        }
    }

    /// <inheritdoc />
    public void DisableCollector(string collectorName)
    {
        if (string.IsNullOrWhiteSpace(collectorName))
        {
            throw new ArgumentException(nameof(collectorName));
        }

        // namespace
        collectorName = collectorName.EnsureNamespace(Namespace);

        if (!Settings.DisabledCollectors.Contains(collectorName))
        {
            Settings.DisabledCollectors.Add(collectorName);
        }
    }

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
        var wageTypeValueType = WageType.ValueType;
        if (valueType.HasValue && Enum.IsDefined(typeof(ValueType), valueType.Value))
        {
            wageTypeValueType = (ValueType)valueType.Value;
        }
        if (!wageTypeValueType.IsNumber())
        {
            throw new ScriptException($"Value type for wage type result must be numeric: {wageTypeValueType}.");
        }

        // culture
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = GetDerivedCulture(DivisionId, Employee.Id);
        }

        // result
        var customResult = new WageTypeCustomResult
        {
            WageTypeNumber = WageTypeNumber,
            WageTypeName = WageTypeName,
            WageTypeNameLocalizations = WageType.NameLocalizations,
            Source = source,
            ValueType = wageTypeValueType,
            Culture = culture,
            Value = value,
            Start = startDate,
            End = endDate,
            Tags = tags,
            Attributes = attributes
        };
        CurrentWageTypeResult.CustomResults.Add(customResult);
    }

    #endregion

    #region Retro Payrun

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