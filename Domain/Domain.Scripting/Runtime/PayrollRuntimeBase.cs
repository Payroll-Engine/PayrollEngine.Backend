using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for a payroll function
/// </summary>
public abstract class PayrollRuntimeBase : RuntimeBase, IPayrollRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected new PayrollRuntimeSettings Settings => base.Settings as PayrollRuntimeSettings;

    /// <summary>
    /// The payroll
    /// </summary>
    protected Payroll Payroll => Settings.Payroll;

    /// <summary>
    /// The case value provider
    /// </summary>
    protected ICaseValueProvider CaseValueProvider => Settings.CaseValueProvider;

    /// <summary>
    /// Provider for regulation lookups
    /// </summary>
    private IRegulationLookupProvider RegulationLookupProvider => Settings.RegulationLookupProvider;

    /// <summary>
    /// Maximum period count
    /// </summary>
    public static readonly int MaxPeriodCount = 200;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollRuntimeBase"/> class
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    protected PayrollRuntimeBase(PayrollRuntimeSettings settings) :
        base(settings)
    {
    }

    #region Employee

    /// <summary>
    /// The employee
    /// </summary>
    protected Employee Employee => CaseValueProvider.Employee;

    /// <inheritdoc />
    public int? EmployeeId => Employee?.Id;

    /// <inheritdoc />
    public string EmployeeIdentifier => Employee?.Identifier;

    /// <inheritdoc />
    public object GetEmployeeAttribute(string attributeName) =>
        Employee?.Attributes?.GetValue<object>(attributeName);

    #endregion

    #region Payroll

    /// <inheritdoc />
    public int PayrollId => Payroll.Id;

    #endregion

    #region Culture

    /// <inheritdoc />
    public virtual string PayrollCulture => Settings.PayrollCulture;

    #endregion

    #region Internal

    /// <summary>
    /// The case value provider
    /// </summary>
    internal DatePeriod EvaluationPeriod => CaseValueProvider.EvaluationPeriod;

    /// <inheritdoc />
    public DateTime EvaluationDate => CaseValueProvider.EvaluationDate;

    /// <summary>Requested case fields</summary>
    internal HashSet<string> RequestedFields { get; } = new();

    #endregion

    #region Period

    /// <inheritdoc />
    public virtual Tuple<DateTime, DateTime> GetEvaluationPeriod() =>
        new(EvaluationPeriod.Start, EvaluationPeriod.End);

    /// <inheritdoc />
    public virtual Tuple<DateTime, DateTime> GetPeriod(DateTime periodMoment, int offset)
    {
        var currentPeriod = CaseValueProvider.PayrollCalculator.GetPayrunPeriod(periodMoment);

        // current period
        if (offset == 0)
        {
            return new(currentPeriod.Start, currentPeriod.End);
        }

        // offset period
        var offsetPeriod = currentPeriod.GetPayrollPeriod(currentPeriod.Start, offset);
        return new(offsetPeriod.Start, offsetPeriod.End);
    }

    #endregion

    #region Cycle

    /// <inheritdoc />
    public virtual Tuple<DateTime, DateTime> GetCycle(DateTime cycleMoment, int offset)
    {
        var currentCycle = CaseValueProvider.PayrollCalculator.GetPayrunCycle(cycleMoment);

        // current cycle
        if (offset == 0)
        {
            return new(currentCycle.Start, currentCycle.End);
        }

        // offset cycle
        var offsetCycle = currentCycle.GetPayrollPeriod(currentCycle.Start, offset);
        return new(offsetCycle.Start, offsetCycle.End);
    }

    #endregion

    #region Case Field and Case Value

    /// <inheritdoc />
    public virtual int? GetCaseValueType(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        var caseField = CaseValueProvider.CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName).Result;
        return caseField != null ? (int)caseField.ValueType : null;
    }

    /// <inheritdoc />
    public virtual object GetCaseFieldAttribute(string caseFieldName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        var caseField = CaseValueProvider.CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName).Result;
        return caseField?.Attributes?.GetValue<object>(attributeName);
    }

    /// <inheritdoc />
    public virtual object GetCaseValueAttribute(string caseFieldName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        var caseField = CaseValueProvider.CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName).Result;
        return caseField?.ValueAttributes?.GetValue<object>(attributeName);
    }

    /// <inheritdoc />
    public virtual List<string> GetCaseValueSlots(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        var caseValueSlots = Task.Run(() => CaseValueProvider.GetCaseValueSlotsAsync(caseFieldName)).Result;
        return caseValueSlots.ToList();
    }

    /// <inheritdoc />
    public virtual List<string> GetCaseValueTags(string caseFieldName, DateTime valueDate)
    {
        var caseValue = GetTimeCaseValue(caseFieldName, valueDate).Result;
        return caseValue == null ? new() : caseValue.Tags;
    }

    /// <inheritdoc />
    public virtual Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>> GetCaseValue(
        string caseFieldName, DateTime valueDate)
    {
        var caseValue = GetTimeCaseValue(caseFieldName, valueDate).Result;
        if (caseValue == null)
        {
            return null;
        }
        return new(caseFieldName,
            caseValue.Created,
            new(caseValue.Start, caseValue.End),
            ValueConvert.ToValue(caseValue.Value, caseValue.ValueType),
            caseValue.CancellationDate,
            caseValue.Tags,
            caseValue.Attributes);
    }

    private async System.Threading.Tasks.Task<CaseValue> GetTimeCaseValue(string caseFieldName, DateTime valueDate)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        var caseField = await CaseValueProvider.CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName);
        if (caseField == null)
        {
            return null;
        }

        var caseType = await CaseValueProvider.CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);
        if (!caseType.HasValue)
        {
            return null;
        }

        var caseValue = (await CaseValueProvider.GetTimeCaseValuesAsync(valueDate, caseType.Value,
            new[] { caseFieldName })).FirstOrDefault();
        return caseValue;
    }

    public virtual List<Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>>> GetCaseValues(
        string caseFieldName, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        startDate ??= Date.MinValue;
        endDate ??= Date.MaxValue;
        if (endDate < startDate)
        {
            throw new ArgumentException($"Invalid period end date: {endDate}", nameof(endDate));
        }

        // period
        var period = new DatePeriod(startDate.Value, endDate.Value);

        // case value periods
        var caseRef = new CaseValueReference(caseFieldName);
        var valuePeriods = Task.Run(() => CaseValueProvider.GetCaseValuesAsync(caseRef.CaseFieldName,
            period, caseRef.CaseSlot)).Result;

        // tuple build
        var values = new List<Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>>>();
        if (valuePeriods != null)
        {
            foreach (var valuePeriod in valuePeriods)
            {
                values.Add(new(valuePeriod.CaseFieldName,
                    valuePeriod.Created,
                    new(valuePeriod.Start, valuePeriod.End),
                    ValueConvert.ToValue(valuePeriod.Value, valuePeriod.ValueType),
                    valuePeriod.CancellationDate,
                    valuePeriod.Tags,
                    valuePeriod.Attributes));
            }
        }
        return values;
    }

    /// <summary>Gets a value indicating whether to track the case field requests</summary>
    private bool TrackCaseFieldRequests => false;

    /// <inheritdoc />
    public virtual Dictionary<string, List<Tuple<DateTime, DateTime?, DateTime?, object>>> GetCasePeriodValues(DateTime startDate,
        DateTime endDate, params string[] caseFieldNames)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException($"Invalid period end date: {endDate}", nameof(endDate));
        }

        // period
        var period = new DatePeriod(startDate, endDate);

        // multiple case period values
        var periodValues = Task.Run(() => CaseValueProvider.GetCasePeriodValuesAsync(period, caseFieldNames)).Result;

        // ensure for any requested a case field value
        var values = caseFieldNames.ToDictionary(
            caseFieldName => caseFieldName,
            _ => new List<Tuple<DateTime, DateTime?, DateTime?, object>>());
        foreach (var periodValue in periodValues)
        {
            values[periodValue.CaseFieldName].Add(new(periodValue.Created,
                periodValue.Start, periodValue.End, ValueConvert.ToValue(periodValue.Value, periodValue.ValueType)));
            if (TrackCaseFieldRequests)
            {
                RequestedFields.Add(periodValue.CaseFieldName);
            }
        }
        return values;
    }

    #endregion

    #region Regulation Lookup

    /// <inheritdoc />
    public virtual string GetLookup(string lookupName, string lookupKey, string culture = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }
        if (string.IsNullOrWhiteSpace(lookupKey))
        {
            throw new ArgumentException(nameof(lookupKey));
        }

        // culture
        culture ??= CultureInfo.CurrentCulture.Name;

        var result = Task.Run(() =>
                RegulationLookupProvider.GetLookupValueDataAsync(Settings.DbContext, lookupName, lookupKey, culture)).Result?.Value;
        return result;
    }

    /// <inheritdoc />
    public virtual string GetRangeLookup(string lookupName, decimal rangeValue, string lookupKey = null, string culture = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }

        // culture
        culture ??= CultureInfo.CurrentCulture.Name;

        return Task.Run(() => RegulationLookupProvider.GetRangeLookupValueDataAsync(Settings.DbContext, lookupName, rangeValue, lookupKey, culture)).
            Result?.Value;
    }

    #endregion

}