using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;
using CaseValue = PayrollEngine.Domain.Model.CaseValue;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides a case value
/// </summary>
public sealed class CaseValueProvider : ICaseValueProvider
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    private CaseValueProviderSettings Settings { get; }

    /// <inheritdoc />
    public Employee Employee { get; }

    /// <inheritdoc />
    public ICaseFieldProvider CaseFieldProvider => Settings.CaseFieldProvider;

    /// <inheritdoc />
    public DateTime EvaluationDate => Settings.EvaluationDate;

    /// <summary>
    /// The retro date
    /// </summary>
    private DateTime? RetroDate => Settings.RetroDate;

    /// <summary>
    /// The global case value repository
    /// </summary>
    private ICaseValueCache GlobalCaseValueRepository { get; }

    /// <summary>
    /// The national case value repository
    /// </summary>
    private ICaseValueCache NationalCaseValueRepository { get; }

    /// <summary>
    /// The company case value repository
    /// </summary>
    private ICaseValueCache CompanyCaseValueRepository { get; }

    /// <summary>
    /// The employee case value repository
    /// </summary>
    private ICaseValueCache EmployeeCaseValueRepository { get; }

    #region ctor

    /// <summary>
    /// Constructor for global case value provider
    /// </summary>
    public CaseValueProvider(
        ICaseValueCache globalCaseValueRepository,
        CaseValueProviderSettings settings) :
        this(settings)
    {
        GlobalCaseValueRepository = globalCaseValueRepository ??
                                    throw new ArgumentNullException(nameof(globalCaseValueRepository));
    }

    /// <summary>
    /// Constructor for national case value provider
    /// </summary>
    public CaseValueProvider(
        ICaseValueCache globalCaseValueRepository,
        ICaseValueCache nationalCaseValueRepository,
        CaseValueProviderSettings settings) :
        this(globalCaseValueRepository, settings)
    {
        NationalCaseValueRepository = nationalCaseValueRepository ??
                                      throw new ArgumentNullException(nameof(nationalCaseValueRepository));
    }

    /// <summary>
    /// Constructor for company case value provider
    /// </summary>
    public CaseValueProvider(
        ICaseValueCache globalCaseValueRepository,
        ICaseValueCache nationalCaseValueRepository,
        ICaseValueCache companyCaseValueRepository,
        CaseValueProviderSettings settings) :
        this(globalCaseValueRepository, nationalCaseValueRepository, settings)
    {
        CompanyCaseValueRepository = companyCaseValueRepository ??
                                     throw new ArgumentNullException(nameof(companyCaseValueRepository));
    }

    /// <summary>
    /// Constructor for employee case value provider
    /// </summary>
    public CaseValueProvider(
        Employee employee,
        ICaseValueCache globalCaseValueRepository,
        ICaseValueCache nationalCaseValueRepository,
        ICaseValueCache companyCaseValueRepository,
        ICaseValueCache employeeCaseValueRepository,
        CaseValueProviderSettings settings) :
        this(globalCaseValueRepository, nationalCaseValueRepository, companyCaseValueRepository, settings)
    {
        Employee = employee ?? throw new ArgumentNullException(nameof(employee));
        EmployeeCaseValueRepository = employeeCaseValueRepository ??
                                      throw new ArgumentNullException(nameof(employeeCaseValueRepository));
    }

    /// <summary>
    /// Constructor for national time period value provider
    /// </summary>
    private CaseValueProvider(CaseValueProviderSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        if (!settings.EvaluationDate.IsUtc())
        {
            throw new ArgumentException("Evaluation date must be UTC.", nameof(settings.EvaluationDate));
        }
        if (!settings.EvaluationPeriod.IsUtc)
        {
            throw new ArgumentException("Evaluation period must be UTC.", nameof(settings.EvaluationPeriod));
        }
        if (settings.RetroDate.HasValue && !settings.RetroDate.Value.IsUtc())
        {
            throw new ArgumentException("Retro date must be UTC.", nameof(settings.RetroDate));
        }

        // initialize the evaluation periods stack
        evaluationPeriods.Push(settings.EvaluationPeriod);
        payrollCalculators.Push(settings.Calculator);
    }

    #endregion

    #region Payroll Calculator

    private readonly Stack<IPayrollCalculator> payrollCalculators = new();

    /// <inheritdoc />
    public IPayrollCalculator PayrollCalculator => payrollCalculators.Peek();

    /// <inheritdoc />
    public void PushCalculator(IPayrollCalculator payrollCalculator)
    {
        if (payrollCalculator == null)
        {
            throw new ArgumentNullException(nameof(payrollCalculator));
        }

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
        if (payrollCalculator == null)
        {
            throw new ArgumentNullException(nameof(payrollCalculator));
        }

        if (payrollCalculator != payrollCalculators.Peek())
        {
            throw new ArgumentException($"Unbalanced stack operation on payroll calculator {payrollCalculator}.",
                nameof(payrollCalculator));
        }

        payrollCalculators.Pop();
    }

    #endregion

    #region Periods

    private readonly Stack<DatePeriod> evaluationPeriods = new();

    /// <inheritdoc />
    public DatePeriod EvaluationPeriod => evaluationPeriods.Peek();

    private void PushEvaluationPeriod(DatePeriod evaluationPeriod)
    {
        //Log.Warning($"Push on count {evaluationPeriods.Count}: {evaluationPeriod}");
        if (evaluationPeriods.Count == 0)
        {
            throw new InvalidOperationException();
        }
        if (!evaluationPeriod.IsUtc)
        {
            throw new ArgumentException("Value date must be UTC.", nameof(evaluationPeriod));
        }
        evaluationPeriods.Push(evaluationPeriod);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private void PopEvaluationPeriod(DatePeriod evaluationPeriod)
    {
        //Log.Warning($"Pop on count {evaluationPeriods.Count}: {evaluationPeriod}");
        if (evaluationPeriod == null)
        {
            throw new ArgumentNullException(nameof(evaluationPeriod));
        }
        if (!evaluationPeriod.IsUtc)
        {
            throw new ArgumentException("Value date must be UTC.", nameof(evaluationPeriod));
        }
        if (evaluationPeriods.Count == 1)
        {
            throw new InvalidOperationException();
        }

        if (evaluationPeriod != evaluationPeriods.Peek())
        {
            throw new ArgumentException("Unbalanced stack operation on evaluation periods.",
                nameof(evaluationPeriod));
        }

        evaluationPeriods.Pop();
    }

    #endregion

    #region Case Value

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCaseValueSlotsAsync(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

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
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType)
    {
        if (!valueDate.IsUtc())
        {
            throw new ArgumentException("Value date must be UTC.", nameof(valueDate));
        }
        var caseFields = (await CaseFieldProvider.GetDerivedCaseFieldsAsync(Settings.DbContext, caseType)).ToList();
        if (!caseFields.Any())
        {
            return [];
        }
        return await GetTimeCaseValuesAsync(valueDate, caseType, caseFields.Select(x => x.Name));
    }

    /// <inheritdoc />
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType,
        IEnumerable<string> caseFieldNames)
    {
        if (!valueDate.IsUtc())
        {
            throw new ArgumentException("Value date must be UTC.", nameof(valueDate));
        }
        var allCaseFieldNames = caseFieldNames.ToList();
        if (!allCaseFieldNames.Any())
        {
            throw new ArgumentNullException(nameof(caseFieldNames));
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
    public async Task<List<CaseFieldValue>> GetCaseValuesAsync(string caseFieldName, DatePeriod evaluationPeriod,
        string caseSlot = null)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (evaluationPeriod == null)
        {
            throw new ArgumentNullException(nameof(evaluationPeriod));
        }
        if (!evaluationPeriod.IsUtc)
        {
            throw new ArgumentException("Value date must be UTC.", nameof(evaluationPeriod));
        }

        // case field
        var caseField = await CaseFieldProvider.GetCaseFieldAsync(Settings.DbContext, caseFieldName);
        if (caseField == null)
        {
            return null;
        }

        // case value repository
        var caseType = await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);
        if (!caseType.HasValue)
        {
            return null;
        }
        var caseValueRepository = GetCaseValueRepository(caseType.Value);

        // case values
        var caseValues = await caseValueRepository.GetCaseValuesAsync(caseFieldName);

        // value periods
        var caseFieldValues = new List<CaseFieldValue>();
        foreach (var caseValue in caseValues)
        {
            // case slot
            if (!string.IsNullOrWhiteSpace(caseSlot) && string.Equals(caseSlot, caseValue.CaseSlot))
            {
                continue;
            }

            // case value periods
            var period = new DatePeriod(caseValue.Start, caseValue.End);
            if (evaluationPeriod.IsWithin(caseValue.Created))
            {
                caseFieldValues.Add(new()
                {
                    CaseFieldName = caseField.Name,
                    CaseFieldNameLocalizations = caseField.NameLocalizations,
                    Created = caseValue.Created,
                    Start = period.Start,
                    End = period.End,
                    ValueType = caseValue.ValueType,
                    Value = caseValue.Value,
                    CancellationDate = caseValue.CancellationDate,
                    Tags = caseValue.Tags,
                    Attributes = caseValue.Attributes
                });
            }
        }
        return caseFieldValues;
    }

    /// <inheritdoc />
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
    public async Task<IList<CaseFieldValue>> GetCasePeriodValuesAsync(
        DatePeriod period, IEnumerable<string> caseFieldNames)
    {
        if (caseFieldNames == null)
        {
            throw new ArgumentException(nameof(caseFieldNames));
        }
        if (!period.IsUtc)
        {
            throw new ArgumentException("Value date must be UTC.", nameof(period));
        }

        var uniqueCaseFieldNames = new HashSet<string>(caseFieldNames);
        if (!uniqueCaseFieldNames.Any())
        {
            throw new ArgumentException(nameof(caseFieldNames));
        }
        var singleFieldRequest = uniqueCaseFieldNames.Count == 1;

        // case values
        var casePeriodValues = new List<CaseFieldValue>();
        PushEvaluationPeriod(period);
        try
        {
            foreach (var caseFieldName in uniqueCaseFieldNames)
            {
                var periodValues = await GetCasePeriodValuesAsync(caseFieldName);
                casePeriodValues.AddRange(periodValues);
            }
        }
        finally
        {
            PopEvaluationPeriod(period);
        }

        // single period request
        if (singleFieldRequest)
        {
            return casePeriodValues;
        }

        // value moments
        var valueMoments = new List<DateTime> { period.Start, period.End };
        foreach (var casePeriodValue in casePeriodValues)
        {
            var periodStart = casePeriodValue.Start;
            if (periodStart.HasValue && periodStart > period.Start && periodStart < period.End &&
                !valueMoments.Contains(periodStart.Value))
            {
                var moment = periodStart.Value.IsLastMomentOfDay() ?
                    periodStart.Value.NextTick() :
                    periodStart.Value;
                valueMoments.Add(moment);
            }

            var periodEnd = casePeriodValue.End;
            if (periodEnd.HasValue && periodEnd.Value > period.Start && periodEnd.Value < period.End &&
                !valueMoments.Contains(periodEnd.Value))
            {
                var moment = periodEnd.Value.IsLastMomentOfDay() ?
                    periodEnd.Value.NextTick() :
                    periodEnd.Value;
                valueMoments.Add(moment);
            }
        }
        // sort from oldest to newest
        valueMoments.Sort((x, y) => x.CompareTo(y));

        // split periods, timeline with moments between start and end
        var splitPeriods = new List<DatePeriod>();
        for (var i = 0; i < valueMoments.Count - 1; i++)
        {
            var periodStart = valueMoments[i];
            var periodEnd = valueMoments[i + 1];
            // at least two ticks required to create a period
            if (Date.IsPeriod(periodStart, periodEnd))
            {
                if (periodEnd.IsMidnight())
                {
                    periodEnd = periodEnd.PreviousTick();
                }
                splitPeriods.Add(new(periodStart.ToUtc(), periodEnd.ToUtc()));
            }
        }

        // no splitting
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
                PushEvaluationPeriod(splitPeriod);
                try
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
                finally
                {
                    PopEvaluationPeriod(splitPeriod);
                }
            }
        }

        return periodCaseValues;
    }

    /// <summary>
    /// Get a case period values by the case field name and type
    /// </summary>
    /// <param name="caseFieldName">The name of the case field</param>
    /// <returns>The case field period values</returns>
    private async Task<IList<CaseFieldValue>> GetCasePeriodValuesAsync(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        var caseType = await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);
        if (!caseType.HasValue)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}.");
        }
        // ReSharper disable once PossibleInvalidOperationException
        var caseValueRepository = GetCaseValueRepository(caseType.Value);
        if (caseValueRepository == null)
        {
            throw new ArgumentNullException(nameof(caseValueRepository));
        }

        // case field
        var values = new List<CaseFieldValue>();
        var caseField = await CaseFieldProvider.GetValueCaseFieldAsync(Settings.DbContext, caseFieldName);
        if (caseField == null)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}.");
        }

        // case values
        var caseValues = (await CalculatePeriodCaseValuesAsync(caseValueRepository, caseFieldName)).ToList();
        if (!caseValues.Any())
        {
            return values;
        }

        // time type timeless
        if (caseField.TimeType == CaseFieldTimeType.Timeless)
        {
            var caseValue = CalculateTimelessCaseValue(caseValues);
            values.Add(new()
            {
                CaseFieldName = caseFieldName,
                CaseFieldNameLocalizations = caseField.NameLocalizations,
                Created = caseValue.Created,
                Start = EvaluationPeriod.Start,
                End = EvaluationPeriod.End,
                ValueType = caseField.ValueType,
                Value = caseValue.Value
            });
            return values;
        }

        // calculator
        var calculator = new CaseValueProviderCalculation(PayrollCalculator, EvaluationDate, EvaluationPeriod);

        // time type moment
        if (caseField.TimeType == CaseFieldTimeType.Moment)
        {
            var value = calculator.CalculatePeriodValue(caseField, caseValues);
            values.Add(new()
            {
                CaseFieldName = caseFieldName,
                CaseFieldNameLocalizations = caseField.NameLocalizations,
                Created = Date.Now,
                Start = EvaluationPeriod.Start,
                End = EvaluationPeriod.End,
                ValueType = caseField.ValueType,
                Value = ValueConvert.ToJson(value)
            });
            return values;
        }

        // get the value periods
        var valuePeriods = calculator.SplitCaseValuePeriods(caseValues);
        if (!valuePeriods.Any())
        {
            return values;
        }

        // period values with split periods
        switch (caseField.TimeType)
        {
            // moment: build summary
            case CaseFieldTimeType.Moment:
                GetMomentCasePeriodValuesAsync(caseFieldName, caseField, valuePeriods, values);
                break;
            case CaseFieldTimeType.Period:
                // period: use the aggregation type
                GetAggregationCasePeriodValuesAsync(caseFieldName, caseField, caseValues, valuePeriods, values);
                break;
            case CaseFieldTimeType.CalendarPeriod:
                GetCalendarPeriodCasePeriodValuesAsync(caseFieldName, caseField, calculator, valuePeriods, values);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return values.OrderBy(x => x.Start).ToList();
    }

    /// <summary>
    /// Get period values from moment case field
    /// </summary>
    /// <param name="caseFieldName">The case field, may include the slot name</param>
    /// <param name="caseField">The case field</param>
    /// <param name="valuePeriods">The value periods</param>
    /// <param name="values">The values</param>
    private static void GetMomentCasePeriodValuesAsync(string caseFieldName, CaseField caseField,
        IDictionary<CaseValue, List<DatePeriod>> valuePeriods, List<CaseFieldValue> values)
    {
        foreach (var valuePeriod in valuePeriods)
        {
            foreach (var datePeriod in valuePeriod.Value)
            {
                var value = valuePeriod.Key.GetValue();
                values.Add(new()
                {
                    CaseFieldName = caseFieldName,
                    CaseFieldNameLocalizations = caseField.NameLocalizations,
                    Created = valuePeriod.Key.Created,
                    Start = datePeriod.Start,
                    End = datePeriod.End,
                    ValueType = caseField.ValueType,
                    Value = ValueConvert.ToJson(value)
                });
            }
        }
    }

    /// <summary>
    /// Get period values from moment case field
    /// </summary>
    /// <param name="caseFieldName">The case field, may include the slot name</param>
    /// <param name="caseField">The case field</param>
    /// <param name="caseValues">The case values</param>
    /// <param name="valuePeriods">The value periods</param>
    /// <param name="values">The values</param>
    private static void GetAggregationCasePeriodValuesAsync(string caseFieldName, CaseField caseField, List<CaseValue> caseValues,
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
                GetMomentCasePeriodValuesAsync(caseFieldName, caseField, valuePeriods, values);
                return;
        }

        // single value selection
        if (valuePeriods.TryGetValue(singleValue, out var datePeriods))
        {
            foreach (var datePeriod in datePeriods)
            {
                values.Add(new()
                {
                    CaseFieldName = caseFieldName,
                    CaseFieldNameLocalizations = caseField.NameLocalizations,
                    Created = singleValue.Created,
                    Start = datePeriod.Start,
                    End = datePeriod.End,
                    ValueType = caseField.ValueType,
                    Value = ValueConvert.ToJson(singleValue.GetValue())
                });
            }
        }
    }

    /// <summary>
    /// Get calendar period values from moment case field
    /// </summary>
    /// <param name="caseFieldName">The case field, may include the slot name</param>
    /// <param name="caseField">The case field</param>
    /// <param name="calculator">The case value calculator</param>
    /// <param name="valuePeriods">The value periods</param>
    /// <param name="values">The values</param>
    private static void GetCalendarPeriodCasePeriodValuesAsync(string caseFieldName, CaseField caseField,
        CaseValueProviderCalculation calculator,
        IDictionary<CaseValue, List<DatePeriod>> valuePeriods,
        List<CaseFieldValue> values)
    {
        foreach (var valuePeriod in valuePeriods)
        {
            foreach (var datePeriod in valuePeriod.Value)
            {
                var value = calculator.CalculateValue(caseField, valuePeriod.Key, datePeriod);
                values.Add(new()
                {
                    CaseFieldName = caseFieldName,
                    CaseFieldNameLocalizations = caseField.NameLocalizations,
                    Created = valuePeriod.Key.Created,
                    Start = datePeriod.Start,
                    End = datePeriod.End.EnsureLastMomentOfDay(),
                    ValueType = caseField.ValueType,
                    Value = ValueConvert.ToJson(value)
                });
            }
        }

        // sort from old start date to new start date
        values.Sort((x, y) => DateTime.Compare(x.Start ?? DateTime.MaxValue, y.Start ?? DateTime.MaxValue));
    }

    #endregion

    #region Case Value

    private ICaseValueCache GetCaseValueRepository(CaseType caseType)
    {
        // repository
        return caseType switch
        {
            CaseType.Global => GlobalCaseValueRepository,
            CaseType.National => NationalCaseValueRepository,
            CaseType.Company => CompanyCaseValueRepository,
            CaseType.Employee => EmployeeCaseValueRepository,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion

    #region Calculation

    /// <summary>
    /// Determine the case value at a specific time
    /// </summary>
    private CaseValue CalculateTimelessCaseValue(IEnumerable<CaseValue> caseValues)
    {
        var evaluationDate = EvaluationDate;
        return caseValues
            // values created before the moment and moment is within the evaluation period
            .Where(cv => cv.Created <= evaluationDate).MaxBy(cv => cv.Created);
    }

    /// <summary>
    /// Tet the case period values
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

    private async Task UpdateRetroCaseValue(ICaseValueCache caseValueRepository, string caseFieldName)
    {
        if (RetroDate.HasValue && RetroDate.Value < EvaluationPeriod.End)
        {
            // retro pay: case value modified after the retro date and the evaluation period end and
            // the value start date is in a previous period and
            // the value start is before the current retro pay case value
            var period = new DatePeriod(RetroDate.Value, EvaluationPeriod.End);
            var retroCaseValue = await caseValueRepository.GetRetroCaseValueAsync(caseFieldName, period);
            if (retroCaseValue != null && (RetroCaseValue == null || retroCaseValue.Start < RetroCaseValue.Start))
            {
                RetroCaseValue = retroCaseValue;
            }
        }
    }

    #endregion

}