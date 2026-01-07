using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting.Runtime;

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

    #region Namespace

    /// <inheritdoc />
    public string Namespace => Settings.Namespace;

    #endregion

    #region Division

    /// <inheritdoc />
    public int DivisionId => Payroll.DivisionId;

    #endregion

    #region Culture & Calendar

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
    internal HashSet<string> RequestedFields { get; } = [];

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

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Settings.Namespace);

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

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Settings.Namespace);

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

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Settings.Namespace);

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

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Settings.Namespace);

        var caseValueSlots = Task.Run(() => CaseValueProvider.GetCaseValueSlotsAsync(caseFieldName)).Result;
        return caseValueSlots.ToList();
    }

    /// <inheritdoc />
    public virtual List<string> GetCaseValueTags(string caseFieldName, DateTime valueDate)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Settings.Namespace);

        var caseValue = GetTimeCaseValue(caseFieldName, valueDate).Result;
        return caseValue == null ? [] : caseValue.Tags;
    }

    /// <inheritdoc />
    public virtual Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>> GetCaseValue(
        string caseFieldName, DateTime valueDate)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Settings.Namespace);

        var caseValue = GetTimeCaseValue(caseFieldName, valueDate).Result;
        if (caseValue == null)
        {
            return null;
        }
        return new(caseFieldName,
            caseValue.Created,
            new(caseValue.Start, caseValue.End),
            ValueConvert.ToValue(caseValue.Value, caseValue.ValueType, TenantCulture),
            caseValue.CancellationDate,
            caseValue.Tags,
            caseValue.Attributes);
    }

    /// <inheritdoc />
    public virtual List<Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>>> GetCaseValues(
        IList<string> caseFieldNames, DateTime valueDate)
    {
        if (caseFieldNames == null)
        {
            throw new ArgumentNullException(nameof(caseFieldNames));
        }

        // namespace
        if (!string.IsNullOrWhiteSpace(Settings.Namespace))
        {
            var fieldNames = new List<string>();
            foreach (var caseFieldName in caseFieldNames)
            {
                fieldNames.Add(caseFieldName.EnsureNamespace(Settings.Namespace));
            }
            caseFieldNames = fieldNames;
        }

        var caseValues = GetTimeCaseValues(caseFieldNames, valueDate).Result;
        var values =
            new List<Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>,
                Dictionary<string, object>>>();

        foreach (var caseValue in caseValues)
        {
            var value = new Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>,
                Dictionary<string, object>>(caseValue.CaseFieldName,
                caseValue.Created,
                new(caseValue.Start, caseValue.End),
                ValueConvert.ToValue(caseValue.Value, caseValue.ValueType, TenantCulture),
                caseValue.CancellationDate,
                caseValue.Tags,
                caseValue.Attributes);
            values.Add(value);
        }
        return values;
    }

    private async System.Threading.Tasks.Task<CaseValue> GetTimeCaseValue(string caseFieldName, DateTime valueDate) =>
        (await GetTimeCaseValues([caseFieldName], valueDate)).FirstOrDefault();

    private async System.Threading.Tasks.Task<List<CaseValue>> GetTimeCaseValues(IList<string> caseFieldNames, DateTime valueDate)
    {
        var caseFieldName = caseFieldNames.FirstOrDefault();
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

        var caseValues = await CaseValueProvider.GetTimeCaseValuesAsync(valueDate, caseType.Value, caseFieldNames);
        return caseValues;
    }

    public virtual List<Tuple<string, DateTime, Tuple<DateTime?, DateTime?>, object, DateTime?, List<string>, Dictionary<string, object>>> GetCaseValues(
        string caseFieldName, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Settings.Namespace);

        startDate ??= Date.MinValue;
        endDate ??= Date.MaxValue;
        if (endDate < startDate)
        {
            throw new ArgumentException($"Invalid period end date: {endDate}.", nameof(endDate));
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
                    ValueConvert.ToValue(valuePeriod.Value, valuePeriod.ValueType, TenantCulture),
                    valuePeriod.CancellationDate,
                    valuePeriod.Tags,
                    valuePeriod.Attributes));
            }
        }
        return values;
    }

    /// <summary>Gets a value indicating whether to track the case field requests</summary>
    private static bool TrackCaseFieldRequests => false;

    /// <inheritdoc />
    public virtual Dictionary<string, List<Tuple<DateTime, DateTime?, DateTime?, object>>> GetCasePeriodValues(DateTime startDate,
        DateTime endDate, params string[] caseFieldNames)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException($"Invalid period end date: {endDate}.", nameof(endDate));
        }

        // namespace
        if (!string.IsNullOrWhiteSpace(Settings.Namespace))
        {
            var fieldNames = new List<string>();
            foreach (var caseFieldName in caseFieldNames)
            {
                fieldNames.Add(caseFieldName.EnsureNamespace(Settings.Namespace));
            }
            caseFieldNames = fieldNames.ToArray();
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
                periodValue.Start, periodValue.End, ValueConvert.ToValue(periodValue.Value, periodValue.ValueType, TenantCulture)));
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
    public bool HasLookup(string lookupName)
    {
        if (string.IsNullOrEmpty(lookupName))
        {
            return false;
        }

        // namespace
        lookupName = lookupName.EnsureNamespace(Settings.Namespace);

        var result = Task.Run(() =>
            RegulationLookupProvider.HasLookupAsync(Settings.DbContext, lookupName)).Result;
        return result;
    }

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

        // namespace
        lookupName = lookupName.EnsureNamespace(Settings.Namespace);

        // culture
        culture ??= CultureInfo.CurrentCulture.Name;

        var result = Task.Run(() =>
                RegulationLookupProvider.GetLookupValueDataAsync(
                    context: Settings.DbContext,
                    lookupName: lookupName,
                    lookupKey: lookupKey,
                    culture: culture)).Result?.Value;
        return result;
    }

    /// <inheritdoc />
    public virtual string GetRangeLookup(string lookupName, decimal rangeValue, string lookupKey = null, string culture = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }

        // namespace
        lookupName = lookupName.EnsureNamespace(Settings.Namespace);

        // culture
        culture ??= CultureInfo.CurrentCulture.Name;

        return Task.Run(() => RegulationLookupProvider.GetRangeLookupValueDataAsync(
                context: Settings.DbContext,
                lookupName: lookupName,
                rangeValue: rangeValue,
                lookupKey: lookupKey,
                culture: culture)).
            Result?.Value;
    }

    /// <inheritdoc />
    public virtual decimal ApplyRangeValue(string lookupName, decimal rangeValue, string valueFieldName = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }

        // range value
        if (rangeValue <= 0)
        {
            return 0;
        }

        // namespace
        lookupName = lookupName.EnsureNamespace(Settings.Namespace);

        var lookup = Task.Run(() => RegulationLookupProvider.GetLookupAsync(
                context: Settings.DbContext,
                lookupName: lookupName)).Result;
        return lookup?.ApplyRangeValue(rangeValue, valueFieldName) ?? 0;
    }

    #endregion

}