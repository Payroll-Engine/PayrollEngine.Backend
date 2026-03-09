using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;
using CaseValue = PayrollEngine.Domain.Model.CaseValue;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Resolves case field values for scripts during payrun execution.
/// Supports all four case type scopes (Global, National, Company, Employee) through
/// a layered set of cache repositories. The active evaluation period is managed via
/// a push/pop stack to allow temporary period overrides during split-period calculations.
/// <para>
/// <b>Threading:</b> This class is not thread-safe. A single instance must only be
/// used from one logical execution context at a time (typically one payrun job).
/// </para>
/// </summary>
public sealed class CaseValueProvider : ICaseValueProvider
{
    /// <summary>
    /// Runtime settings containing the DB context, calculator, case field provider,
    /// evaluation period/date and optional retro date.
    /// </summary>
    private CaseValueProviderSettings Settings { get; }

    /// <inheritdoc />
    public Employee Employee { get; }

    /// <inheritdoc />
    public ICaseFieldProvider CaseFieldProvider => Settings.CaseFieldProvider;

    /// <inheritdoc />
    public DateTime EvaluationDate => Settings.EvaluationDate;

    /// <summary>
    /// The retro date; when set, case value changes after this date trigger a retro payrun.
    /// </summary>
    private DateTime? RetroDate => Settings.RetroDate;

    /// <summary>Cache for tenant-wide global case values.</summary>
    private ICaseValueCache GlobalCaseValueRepository { get; }

    /// <summary>Cache for nation-level case values.</summary>
    private ICaseValueCache NationalCaseValueRepository { get; }

    /// <summary>Cache for company-level case values.</summary>
    private ICaseValueCache CompanyCaseValueRepository { get; }

    /// <summary>Cache for employee-specific case values; <c>null</c> in non-employee contexts.</summary>
    private ICaseValueCache EmployeeCaseValueRepository { get; }

    #region ctor

    /// <summary>
    /// Creates a provider with access to the specified case value scopes.
    /// Only <paramref name="settings"/> is required. Provide cache repositories for each
    /// scope your scripts will access; accessing a scope without a configured repository
    /// throws an <see cref="InvalidOperationException"/> with a descriptive message.
    /// </summary>
    /// <param name="settings">Shared provider settings (required).</param>
    /// <param name="globalCaseValueRepository">Cache for global case values.</param>
    /// <param name="nationalCaseValueRepository">Cache for national case values.</param>
    /// <param name="companyCaseValueRepository">Cache for company case values.</param>
    /// <param name="employeeCaseValueRepository">Cache for employee-specific case values.</param>
    /// <param name="employee">The employee whose case values are resolved; required when using employee scope.</param>
    public CaseValueProvider(
        CaseValueProviderSettings settings,
        ICaseValueCache globalCaseValueRepository = null,
        ICaseValueCache nationalCaseValueRepository = null,
        ICaseValueCache companyCaseValueRepository = null,
        ICaseValueCache employeeCaseValueRepository = null,
        Employee employee = null) : this(settings)
    {
        GlobalCaseValueRepository = globalCaseValueRepository;
        NationalCaseValueRepository = nationalCaseValueRepository;
        CompanyCaseValueRepository = companyCaseValueRepository;
        EmployeeCaseValueRepository = employeeCaseValueRepository;
        Employee = employee;
    }

    /// <summary>
    /// Base constructor; validates all UTC constraints and initialises the
    /// evaluation period and calculator stacks with the values from <paramref name="settings"/>.
    /// Called by every public constructor via chaining.
    /// </summary>
    /// <param name="settings">Shared provider settings.</param>
    private CaseValueProvider(CaseValueProviderSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        settings.EvaluationDate.EnsureUtc();
        settings.EvaluationPeriod.EnsureUtc();
        settings.RetroDate.EnsureUtc();

        // initialize the evaluation periods stack
        evaluationPeriods.Push(settings.EvaluationPeriod);
        payrollCalculators.Push(settings.Calculator);
    }

    #endregion

    #region Payroll Calculator

    /// <summary>
    /// Stack of active payroll calculators. The bottom entry is the payrun-level calculator;
    /// wage types with a custom calendar push/pop their own calculator on top.
    /// </summary>
    private readonly Stack<IPayrollCalculator> payrollCalculators = new();

    /// <inheritdoc />
    public IPayrollCalculator PayrollCalculator => payrollCalculators.Peek();

    /// <inheritdoc />
    public void PushCalculator(IPayrollCalculator payrollCalculator)
    {
        ArgumentNullException.ThrowIfNull(payrollCalculator);

        if (payrollCalculators.Any())
        {
            // test cycle compatibility
            var curCycleTimeUnit = payrollCalculators.Peek().CycleTimeUnit;
            var newCycleTimeUnit = payrollCalculator.CycleTimeUnit;
            if (!curCycleTimeUnit.IsValidTimeUnit(newCycleTimeUnit))
            {
                throw new PayrollException($"Mismatching cycle time type {newCycleTimeUnit}, must be compatible with {curCycleTimeUnit}.");
            }

            // test period compatibility
            var curPeriodTimeUnit = payrollCalculators.Peek().PeriodTimeUnit;
            var newPeriodTimeUnit = payrollCalculator.PeriodTimeUnit;
            if (!curPeriodTimeUnit.IsValidTimeUnit(newPeriodTimeUnit))
            {
                throw new PayrollException($"Mismatching period time type {newPeriodTimeUnit}, must be compatible with {curPeriodTimeUnit}.");
            }
        }

        payrollCalculators.Push(payrollCalculator);
    }

    /// <inheritdoc />
    public void PopCalculator(IPayrollCalculator payrollCalculator)
    {
        ArgumentNullException.ThrowIfNull(payrollCalculator);

        if (payrollCalculator != payrollCalculators.Peek())
        {
            throw new ArgumentException($"Unbalanced stack operation on payroll calculator {payrollCalculator}.",
                nameof(payrollCalculator));
        }

        payrollCalculators.Pop();
    }

    #endregion

    #region Periods

    /// <summary>
    /// Stack of active evaluation periods. The bottom entry is the payrun job period;
    /// split-period calculations temporarily push sub-periods on top.
    /// </summary>
    private readonly Stack<DatePeriod> evaluationPeriods = new();

    /// <inheritdoc />
    public DatePeriod EvaluationPeriod => evaluationPeriods.Peek();

    /// <summary>
    /// Creates a scoped evaluation period override. The period is pushed onto the stack
    /// and automatically popped when the returned scope is disposed.
    /// <para>Usage: <c>using (UseEvaluationPeriod(period)) { ... }</c></para>
    /// </summary>
    /// <param name="evaluationPeriod">The period to make active; must be UTC.</param>
    /// <returns>An <see cref="IDisposable"/> that pops the period on dispose.</returns>
    private IDisposable UseEvaluationPeriod(DatePeriod evaluationPeriod)
    {
        if (evaluationPeriods.Count == 0)
        {
            throw new InvalidOperationException("Cannot push: evaluation period stack is empty.");
        }
        evaluationPeriod.EnsureUtc();
        evaluationPeriods.Push(evaluationPeriod);
        return new DisposableAction(() =>
        {
            if (evaluationPeriod != evaluationPeriods.Peek())
            {
                throw new InvalidOperationException("Unbalanced stack operation on evaluation periods.");
            }
            evaluationPeriods.Pop();
        });
    }

    #endregion

    #region Case Field Value Factory

    /// <summary>
    /// Creates a <see cref="CaseFieldValue"/> from a case field definition and a source case value.
    /// Centralizes the repeated object-initializer pattern found throughout the provider.
    /// </summary>
    /// <param name="caseFieldName">The case field name (may include a slot suffix).</param>
    /// <param name="caseField">The resolved case field definition.</param>
    /// <param name="created">The creation timestamp to assign.</param>
    /// <param name="start">The period start date.</param>
    /// <param name="end">The period end date.</param>
    /// <param name="value">The JSON-encoded value string.</param>
    /// <returns>A new <see cref="CaseFieldValue"/> instance.</returns>
    private static CaseFieldValue CreateFieldValue(
        string caseFieldName, CaseField caseField,
        DateTime created, DateTime? start, DateTime? end,
        string value) => new()
        {
            CaseFieldName = caseFieldName,
            CaseFieldNameLocalizations = caseField.NameLocalizations,
            Created = created,
            Start = start,
            End = end?.EnsureLastMomentOfDay(),
            ValueType = caseField.ValueType,
            Value = value
        };

    /// <summary>
    /// Creates a <see cref="CaseFieldValue"/> with additional metadata fields
    /// (cancellation date, tags, attributes) copied from the source <paramref name="caseValue"/>.
    /// Used when the full case value context must be preserved in the result.
    /// </summary>
    private static CaseFieldValue CreateFieldValueWithMetadata(
        CaseField caseField, CaseValue caseValue,
        DateTime? start, DateTime? end) => new()
        {
            CaseFieldName = caseField.Name,
            CaseFieldNameLocalizations = caseField.NameLocalizations,
            Created = caseValue.Created,
            Start = start,
            End = end,
            ValueType = caseValue.ValueType,
            Value = caseValue.Value,
            CancellationDate = caseValue.CancellationDate,
            Tags = caseValue.Tags,
            Attributes = caseValue.Attributes
        };

    #endregion

    #region Case Value

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// <paramref name="caseFieldName"/> is <c>null</c>, empty or whitespace.
    /// </exception>
    /// <exception cref="PayrollException">
    /// The case field name does not resolve to a known case type.
    /// </exception>
    public async Task<IEnumerable<string>> GetCaseValueSlotsAsync(string caseFieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseFieldName);

        // case
        var caseType = await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);
        if (!caseType.HasValue)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}.");
        }

        // case value
        var caseValueRepository = GetCaseValueRepository(caseType.Value);
        return await caseValueRepository.GetCaseValueSlotsAsync(caseFieldName);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// <paramref name="valueDate"/> is not in UTC.
    /// </exception>
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType)
    {
        valueDate.EnsureUtc();
        var caseFields = (await CaseFieldProvider.GetDerivedCaseFieldsAsync(Settings.DbContext, caseType)).ToList();
        if (!caseFields.Any())
        {
            return [];
        }
        return await GetTimeCaseValuesAsync(valueDate, caseType, caseFields.Select(x => x.Name));
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// <paramref name="valueDate"/> is not UTC, or <paramref name="caseFieldNames"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="caseFieldNames"/> is <c>null</c>.
    /// </exception>
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType,
        IEnumerable<string> caseFieldNames)
    {
        valueDate.EnsureUtc();
        ArgumentNullException.ThrowIfNull(caseFieldNames);
        var allCaseFieldNames = caseFieldNames.ToList();
        if (!allCaseFieldNames.Any())
        {
            throw new ArgumentException("At least one case field name is required.", nameof(caseFieldNames));
        }

        // collect case field values
        var caseValues = new List<CaseValue>();
        foreach (var caseFieldName in allCaseFieldNames)
        {
            // case field check
            var caseField = await CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName);
            if (caseField == null)
            {
                // unknown case field
                continue;
            }
            var caseFieldType = await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);
            if (!caseFieldType.HasValue || caseFieldType.Value != caseType)
            {
                // invalid case type
                continue;
            }

            // case value repository
            var caseValueRepository = GetCaseValueRepository(caseFieldType.Value);

            // all case values
            var caseFieldValues = (await caseValueRepository.GetCaseValuesAsync(caseFieldName)).ToList();
            if (!caseFieldValues.Any())
            {
                continue;
            }

            // select the last created case value
            CaseValue caseValue = null;
            switch (caseField.TimeType)
            {
                case CaseFieldTimeType.Timeless:
                    // the latest created
                    caseValue = caseFieldValues.MaxBy(x => x.Created);
                    break;
                case CaseFieldTimeType.Moment:
                    // the latest moment/start case value before the value date (ignore forecast values)
                    caseValue = caseFieldValues.Where(x => x.Start.HasValue && x.Start <= valueDate).MaxBy(x => x.Start);
                    break;
                case CaseFieldTimeType.Period:
                case CaseFieldTimeType.CalendarPeriod:
                    // the last created case value period including the value date
                    caseValue = caseFieldValues.Where(x => x.GetPeriod().IsWithin(valueDate)).MaxBy(x => x.Created);
                    break;
            }
            if (caseValue != null)
            {
                caseValues.Add(caseValue);
            }
        }

        return caseValues;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// <paramref name="caseFieldName"/> is <c>null</c>, empty or whitespace,
    /// or <paramref name="evaluationPeriod"/> is not UTC.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="evaluationPeriod"/> is <c>null</c>.
    /// </exception>
    public async Task<List<CaseFieldValue>> GetCaseValuesAsync(string caseFieldName,
        DatePeriod evaluationPeriod, string caseSlot = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseFieldName);
        evaluationPeriod.EnsureUtc();

        // case field
        var caseField = await CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName);
        if (caseField == null)
        {
            return null;
        }

        // case value repository
        var caseType = await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseField.Name);
        if (!caseType.HasValue)
        {
            return null;
        }
        var caseValueRepository = GetCaseValueRepository(caseType.Value);

        // case values
        var caseValues = await caseValueRepository.GetCaseValuesAsync(caseField.Name);

        // value periods
        var caseFieldValues = new List<CaseFieldValue>();
        foreach (var caseValue in caseValues)
        {
            // case slot filter: skip values that do not belong to the requested slot
            if (!string.IsNullOrWhiteSpace(caseSlot) &&
                !string.Equals(caseSlot, caseValue.CaseSlot, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // case value periods
            if (evaluationPeriod.IsWithin(caseValue.Created))
            {
                var period = new DatePeriod(caseValue.Start, caseValue.End);
                caseFieldValues.Add(CreateFieldValueWithMetadata(
                    caseField, caseValue, period.Start, period.End));
            }
        }
        return caseFieldValues;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// No case value repository is configured for the requested <paramref name="caseType"/> scope.
    /// </exception>
    public async Task<IDictionary<CaseValue, List<DatePeriod>>> GetCaseValueSplitPeriodsAsync(
        string caseFieldName, CaseType caseType, string caseSlot = null)
    {
        var caseValueRepository = GetCaseValueRepository(caseType);
        var caseValues = (await caseValueRepository.GetCaseValuesAsync(
            CaseValueReference.ToReference(caseFieldName, caseSlot))).ToList();
        if (caseValues.Any())
        {
            var calculator = new CaseValueProviderCalculation(PayrollCalculator, EvaluationDate, EvaluationPeriod);
            return calculator.SplitCaseValuePeriods(caseValues);
        }
        return new Dictionary<CaseValue, List<DatePeriod>>();
    }

    #endregion

    #region Period Values

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// <paramref name="period"/> or <paramref name="caseFieldNames"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="caseFieldNames"/> is empty, or <paramref name="period"/> is not UTC.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// A split sub-period produced more than one value for a single field.
    /// </exception>
    public async Task<IList<CaseFieldValue>> GetCasePeriodValuesAsync(
        DatePeriod period, IEnumerable<string> caseFieldNames)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(caseFieldNames);
        period.EnsureUtc();

        // use a list with preserved insertion order (HashSet has no guaranteed iteration order)
        var uniqueCaseFieldNames = caseFieldNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (!uniqueCaseFieldNames.Any())
        {
            throw new ArgumentException("At least one case field name is required.", nameof(caseFieldNames));
        }
        var singleFieldRequest = uniqueCaseFieldNames.Count == 1;

        // case values
        var casePeriodValues = new List<CaseFieldValue>();
        using (UseEvaluationPeriod(period))
        {
            foreach (var caseFieldName in uniqueCaseFieldNames)
            {
                var periodValues = await GetCasePeriodValuesAsync(caseFieldName);
                casePeriodValues.AddRange(periodValues);
            }
        }

        // single period request
        if (singleFieldRequest)
        {
            return casePeriodValues;
        }

        // calculate split periods from all case field value boundaries
        var splitPeriods = CalculateSplitPeriods(period, casePeriodValues);

        // no splitting needed
        if (splitPeriods.Count == 1)
        {
            return casePeriodValues;
        }

        // step 2: load case values by split periods
        var periodCaseValues = new List<CaseFieldValue>();
        foreach (var caseFieldName in uniqueCaseFieldNames)
        {
            foreach (var splitPeriod in splitPeriods)
            {
                using (UseEvaluationPeriod(splitPeriod))
                {
                    var periodValues = await GetCasePeriodValuesAsync(caseFieldName);
                    if (periodValues.Any())
                    {
                        if (periodValues.Count != 1)
                        {
                            throw new InvalidOperationException("Expecting only one splitting period value");
                        }
                        var periodValue = periodValues.First();
                        periodCaseValues.Add(new()
                        {
                            CaseFieldName = caseFieldName,
                            Created = periodValue.Created,
                            Start = splitPeriod.Start,
                            End = splitPeriod.End,
                            ValueType = periodValue.ValueType,
                            Value = periodValue.Value
                        });
                    }
                }
            }
        }

        return periodCaseValues;
    }

    /// <summary>
    /// Calculates the split periods from case field value boundaries within the given period.
    /// Collects all start/end moments, sorts them chronologically, and builds sub-periods
    /// from adjacent moment pairs. This is the core algorithm for multi-field period splitting.
    /// </summary>
    /// <param name="period">The outer evaluation period defining the boundaries.</param>
    /// <param name="casePeriodValues">The case field values whose start/end dates define split points.</param>
    /// <returns>A list of non-overlapping sub-periods covering the original period.</returns>
    private static List<DatePeriod> CalculateSplitPeriods(
        DatePeriod period, IList<CaseFieldValue> casePeriodValues)
    {
        // collect unique moments from all case field value boundaries
        var moments = new SortedSet<DateTime> { period.Start, period.End };
        foreach (var casePeriodValue in casePeriodValues)
        {
            AddMomentIfInRange(moments, casePeriodValue.Start, period);
            AddMomentIfInRange(moments, casePeriodValue.End, period);
        }

        // build periods from adjacent moment pairs
        var splitPeriods = new List<DatePeriod>();
        var momentList = moments.ToList();
        for (var i = 0; i < momentList.Count - 1; i++)
        {
            var periodStart = momentList[i];
            var periodEnd = momentList[i + 1];
            // at least two ticks required to create a period
            if (!Date.IsPeriod(periodStart, periodEnd))
            {
                continue;
            }
            if (periodEnd.IsMidnight())
            {
                periodEnd = periodEnd.PreviousTick();
            }
            Debug.Assert(periodStart < periodEnd,
                $"Invalid split period after tick correction: {periodStart} >= {periodEnd}");
            splitPeriods.Add(new(periodStart.ToUtc(), periodEnd.ToUtc()));
        }
        return splitPeriods;
    }

    /// <summary>
    /// Adds a date/time value as a moment to the sorted set if it falls strictly within
    /// the given period boundaries. Adjusts last-moment-of-day values to the next tick.
    /// </summary>
    private static void AddMomentIfInRange(SortedSet<DateTime> moments, DateTime? value, DatePeriod period)
    {
        if (!value.HasValue || value.Value <= period.Start || value.Value >= period.End)
        {
            return;
        }
        var moment = value.Value.IsLastMomentOfDay()
            ? value.Value.NextTick()
            : value.Value;
        moments.Add(moment);
    }

    /// <summary>
    /// Resolves all period values for a single case field within the currently active
    /// evaluation period (top of <see cref="evaluationPeriods"/> stack). Dispatches to the
    /// appropriate calculation path based on the field's <see cref="CaseFieldTimeType"/>.
    /// </summary>
    /// <param name="caseFieldName">The case field name (may include a slot suffix).</param>
    /// <returns>The resolved <see cref="CaseFieldValue"/> entries for the current evaluation period.</returns>
    private async Task<IList<CaseFieldValue>> GetCasePeriodValuesAsync(string caseFieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseFieldName);

        var caseType = await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);
        if (!caseType.HasValue)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}.");
        }
        // ReSharper disable once PossibleInvalidOperationException
        var caseValueRepository = GetCaseValueRepository(caseType.Value);

        // case field
        var caseField = await CaseFieldProvider.GetValueCaseFieldAsync(Settings.DbContext, caseFieldName);
        if (caseField == null)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}.");
        }

        // case values
        var caseValues = (await CalculatePeriodCaseValuesAsync(caseValueRepository, caseFieldName)).ToList();
        if (!caseValues.Any())
        {
            return [];
        }

        // dispatch by time type
        var resolved = caseField.TimeType switch
        {
            CaseFieldTimeType.Timeless => ResolveTimeless(caseFieldName, caseField, caseValues),
            CaseFieldTimeType.Moment => ResolveMoment(caseFieldName, caseField, caseValues),
            CaseFieldTimeType.Period => ResolvePeriodType(caseFieldName, caseField, caseValues),
            CaseFieldTimeType.CalendarPeriod => ResolveCalendarPeriod(caseFieldName, caseField, caseValues),
            _ => throw new ArgumentOutOfRangeException(nameof(caseField.TimeType), caseField.TimeType, null)
        };
        return resolved;
    }

    /// <summary>
    /// Resolves a timeless case field: returns the most recently created value
    /// that was created on or before the evaluation date.
    /// </summary>
    private IList<CaseFieldValue> ResolveTimeless(
        string caseFieldName, CaseField caseField, List<CaseValue> caseValues)
    {
        var caseValue = CalculateTimelessCaseValue(caseValues);
        if (caseValue == null)
        {
            return [];
        }
        return
        [
            CreateFieldValue(
                caseFieldName, caseField, caseValue.Created,
                EvaluationPeriod.Start, EvaluationPeriod.End,
                caseValue.Value)
        ];
    }

    /// <summary>
    /// Resolves a moment-type case field: returns the calculated period value
    /// using the latest start date that is ≤ the evaluation period end.
    /// </summary>
    private IList<CaseFieldValue> ResolveMoment(
        string caseFieldName, CaseField caseField, List<CaseValue> caseValues)
    {
        var calculator = new CaseValueProviderCalculation(PayrollCalculator, EvaluationDate, EvaluationPeriod);
        var value = calculator.CalculatePeriodValue(caseField, caseValues);
        var momentCreated = caseValues
            .Where(x => x.Start.HasValue && x.Start <= EvaluationPeriod.End)
            .MaxBy(x => x.Start)?.Created ?? caseValues.Max(x => x.Created);
        return
        [
            CreateFieldValue(
                caseFieldName, caseField, momentCreated,
                EvaluationPeriod.Start, EvaluationPeriod.End,
                ValueConvert.ToJson(value))
        ];
    }

    /// <summary>
    /// Resolves a period-type case field: splits case values into sub-periods and
    /// applies the field's <see cref="CaseField.PeriodAggregation"/> rule.
    /// </summary>
    private IList<CaseFieldValue> ResolvePeriodType(
        string caseFieldName, CaseField caseField, List<CaseValue> caseValues)
    {
        var calculator = new CaseValueProviderCalculation(PayrollCalculator, EvaluationDate, EvaluationPeriod);
        var valuePeriods = calculator.SplitCaseValuePeriods(caseValues);
        if (!valuePeriods.Any())
        {
            return [];
        }
        var values = new List<CaseFieldValue>();
        GetAggregationCasePeriodValues(caseFieldName, caseField, caseValues, valuePeriods, values);
        return values.OrderBy(x => x.Start).ToList();
    }

    /// <summary>
    /// Resolves a calendar-period-type case field: splits case values into sub-periods and
    /// pro-rates each value according to the calendar period fraction.
    /// </summary>
    private IList<CaseFieldValue> ResolveCalendarPeriod(
        string caseFieldName, CaseField caseField, List<CaseValue> caseValues)
    {
        var calculator = new CaseValueProviderCalculation(PayrollCalculator, EvaluationDate, EvaluationPeriod);
        var valuePeriods = calculator.SplitCaseValuePeriods(caseValues);
        if (!valuePeriods.Any())
        {
            return [];
        }
        var values = new List<CaseFieldValue>();
        GetCalendarPeriodCasePeriodValues(caseFieldName, caseField, calculator, valuePeriods, values);
        return values.OrderBy(x => x.Start).ToList();
    }

    /// <summary>
    /// Builds <see cref="CaseFieldValue"/> entries from a moment-type case field by projecting
    /// each case value onto its effective date periods.
    /// </summary>
    private static void GetMomentCasePeriodValues(string caseFieldName, CaseField caseField,
        IDictionary<CaseValue, List<DatePeriod>> valuePeriods, List<CaseFieldValue> values)
    {
        foreach (var valuePeriod in valuePeriods)
        {
            foreach (var datePeriod in valuePeriod.Value)
            {
                values.Add(CreateFieldValue(
                    caseFieldName, caseField, valuePeriod.Key.Created,
                    datePeriod.Start, datePeriod.End,
                    ValueConvert.ToJson(valuePeriod.Key.GetValue())));
            }
        }
    }

    /// <summary>
    /// Builds <see cref="CaseFieldValue"/> entries from a period-type case field, applying the
    /// field's <see cref="CaseField.PeriodAggregation"/> rule (<c>First</c>, <c>Last</c>, or
    /// <c>Summary</c>) to select which case value(s) contribute to the result.
    /// </summary>
    private static void GetAggregationCasePeriodValues(string caseFieldName, CaseField caseField, List<CaseValue> caseValues,
        IDictionary<CaseValue, List<DatePeriod>> valuePeriods, List<CaseFieldValue> values)
    {
        CaseValue singleValue;
        switch (caseField.PeriodAggregation)
        {
            // first
            case CaseFieldAggregationType.First:
                singleValue = caseValues.OrderBy(x => x.Created).First();
                break;

            // last
            case CaseFieldAggregationType.Last:
                singleValue = caseValues.OrderBy(x => x.Created).Last();
                break;

            // summary as default
            case CaseFieldAggregationType.Summary:
            default:
                GetMomentCasePeriodValues(caseFieldName, caseField, valuePeriods, values);
                return;
        }

        // single value selection
        if (valuePeriods.TryGetValue(singleValue, out var datePeriods))
        {
            foreach (var datePeriod in datePeriods)
            {
                values.Add(CreateFieldValue(
                    caseFieldName, caseField, singleValue.Created,
                    datePeriod.Start, datePeriod.End,
                    ValueConvert.ToJson(singleValue.GetValue())));
            }
        }
    }

    /// <summary>
    /// Builds <see cref="CaseFieldValue"/> entries from a calendar-period-type case field.
    /// The value for each sub-period is computed by the calculator,
    /// which pro-rates the raw case value according to the calendar period fraction.
    /// <para>
    /// Unlike <see cref="GetMomentCasePeriodValues"/> and <see cref="GetAggregationCasePeriodValues"/>,
    /// this method normalizes <c>End</c> via <c>EnsureLastMomentOfDay</c>.
    /// Calendar periods are day-boundary-based, so end dates must be aligned to 23:59:59.9999999
    /// to ensure correct pro rata calculations. Period and Moment types use raw end dates as-is.
    /// </para>
    /// </summary>
    private static void GetCalendarPeriodCasePeriodValues(string caseFieldName, CaseField caseField,
        CaseValueProviderCalculation calculator,
        IDictionary<CaseValue, List<DatePeriod>> valuePeriods,
        List<CaseFieldValue> values)
    {
        foreach (var valuePeriod in valuePeriods)
        {
            foreach (var datePeriod in valuePeriod.Value)
            {
                var value = calculator.CalculateValue(caseField, valuePeriod.Key, datePeriod);
                values.Add(CreateFieldValue(
                    caseFieldName, caseField, valuePeriod.Key.Created,
                    datePeriod.Start, datePeriod.End.EnsureLastMomentOfDay(),
                    ValueConvert.ToJson(value)));
            }
        }

        // sorting is handled by the caller (GetCasePeriodValuesAsync)
    }

    #endregion

    #region Case Value

    /// <summary>
    /// Returns the cache repository that corresponds to the given <paramref name="caseType"/>.
    /// </summary>
    private ICaseValueCache GetCaseValueRepository(CaseType caseType)
    {
        // repository
        var repository = caseType switch
        {
            CaseType.Global => GlobalCaseValueRepository,
            CaseType.National => NationalCaseValueRepository,
            CaseType.Company => CompanyCaseValueRepository,
            CaseType.Employee => EmployeeCaseValueRepository,
            _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, $"Unknown case type {caseType}.")
        };
        return repository ?? throw new InvalidOperationException(
            $"No case value repository configured for case type {caseType}. " +
            $"Ensure the provider was constructed with a repository for this scope.");
    }

    #endregion

    #region Calculation

    /// <summary>
    /// Returns the most recently created case value that was created on or before the
    /// evaluation date. Used for <see cref="CaseFieldTimeType.Timeless"/> fields where
    /// no period boundary applies.
    /// </summary>
    private CaseValue CalculateTimelessCaseValue(IEnumerable<CaseValue> caseValues)
    {
        var evaluationDate = EvaluationDate;
        return caseValues
            // values created before the moment and moment is within the evaluation period
            .Where(cv => cv.Created <= evaluationDate).MaxBy(cv => cv.Created);
    }

    /// <summary>
    /// Loads the case values for the active evaluation period from the cache and
    /// simultaneously checks for a retro trigger on the same field.
    /// </summary>
    private async Task<IEnumerable<CaseValue>> CalculatePeriodCaseValuesAsync(ICaseValueCache caseValueRepository,
        string caseFieldName)
    {
        var caseValues = (await caseValueRepository.GetCasePeriodValuesAsync(caseFieldName)).ToList();
        await UpdateRetroCaseValue(caseValueRepository, caseFieldName);
        return caseValues;
    }

    #endregion

    #region Retro

    /// <inheritdoc />
    public CaseValue RetroCaseValue { get; private set; }

    /// <summary>
    /// Checks whether a retro trigger exists for the given case field and, if so, updates
    /// <see cref="RetroCaseValue"/> with the earliest qualifying value found.
    /// </summary>
    private async Task UpdateRetroCaseValue(ICaseValueCache caseValueRepository, string caseFieldName)
    {
        if (RetroDate.HasValue && RetroDate.Value < EvaluationPeriod.End)
        {
            // period start is before the current retro pay case value
            var periodStart = RetroDate.Value;
            // period end or evaluation date
            var periodEnd = EvaluationPeriod.End;
            if (EvaluationPeriod.IsWithin(EvaluationDate))
            {
                periodEnd = EvaluationDate;
            }
            var period = new DatePeriod(periodStart, periodEnd);

            // query cached retro values
            var retroCaseValue = await caseValueRepository.GetRetroCaseValueAsync(caseFieldName, period);
            if (retroCaseValue != null && (RetroCaseValue == null || retroCaseValue.Start < RetroCaseValue.Start))
            {
                RetroCaseValue = retroCaseValue;
            }
        }
    }

    #endregion

}
