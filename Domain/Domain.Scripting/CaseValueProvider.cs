using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using CaseValue = PayrollEngine.Domain.Model.CaseValue;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides a case value
/// </summary>
public sealed class CaseValueProvider
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    public CaseValueProviderSettings Settings { get; }

    /// <summary>
    /// The employee
    /// </summary>
    public Employee Employee { get; }

    /// <summary>
    /// The case field provider
    /// </summary>
    public CaseFieldProvider CaseFieldProvider => Settings.CaseFieldProvider;

    /// <summary>
    /// The evaluation date
    /// </summary>
    public DateTime EvaluationDate => Settings.EvaluationDate;

    /// <summary>
    /// The retro date
    /// </summary>
    public DateTime? RetroDate => Settings.RetroDate;

    /// <summary>
    /// The global case value repository
    /// </summary>
    public ICaseValueCache GlobalCaseValueRepository { get; }

    /// <summary>
    /// The national case value repository
    /// </summary>
    public ICaseValueCache NationalCaseValueRepository { get; }

    /// <summary>
    /// The company case value repository
    /// </summary>
    public ICaseValueCache CompanyCaseValueRepository { get; }

    /// <summary>
    /// The employee case value repository
    /// </summary>
    public ICaseValueCache EmployeeCaseValueRepository { get; }

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
            throw new ArgumentException("Evaluation date must be UTC", nameof(settings.EvaluationDate));
        }
        if (!settings.EvaluationPeriod.IsUtc)
        {
            throw new ArgumentException("Evaluation period must be UTC", nameof(settings.EvaluationPeriod));
        }
        if (settings.RetroDate.HasValue && !settings.RetroDate.Value.IsUtc())
        {
            throw new ArgumentException("Retro date must be UTC", nameof(settings.RetroDate));
        }

        // initialize the evaluation periods stack
        evaluationPeriods.Push(settings.EvaluationPeriod);
        payrollCalculators.Push(settings.Calculator);
    }

    #endregion

    #region Payroll Calculator

    private readonly Stack<IPayrollCalculator> payrollCalculators = new();

    /// <summary>
    /// The current payroll calculator
    /// </summary>
    public IPayrollCalculator PayrollCalculator => payrollCalculators.Peek();

    /// <summary>
    /// Push the current payroll calculator
    /// </summary>
    /// <param name="payrollCalculator">The new payroll calculator</param>
    public void PushCalculator(IPayrollCalculator payrollCalculator) =>
        payrollCalculators.Push(payrollCalculator);

    /// <summary>
    /// Pop the current payroll calculator
    /// </summary>
    /// <param name="payrollCalculator">The new payroll calculator</param>
    public void PopCalculator(IPayrollCalculator payrollCalculator)
    {
        if (payrollCalculator == null)
        {
            throw new ArgumentNullException(nameof(payrollCalculator));
        }

        if (payrollCalculator != payrollCalculators.Peek())
        {
            throw new ArgumentException($"Unbalanced stack operation on payroll calculator {payrollCalculator}",
                nameof(payrollCalculator));
        }

        payrollCalculators.Pop();
    }

    #endregion

    #region Periods

    /// <summary>
    /// Get offset period
    /// </summary>
    /// <param name="moment">A moment within the payrun period</param>
    /// <param name="offset">The period offset count</param>
    /// <returns>The offset period</returns>
    public DatePeriod GetOffsetCycle(DateTime moment, int offset)
    {
        if (!moment.IsUtc())
        {
            throw new ArgumentException("Value date must be UTC", nameof(moment));
        }
        var payrollPeriod = PayrollCalculator.GetPayrunCycle(moment);
        return payrollPeriod.GetOffsetDatePeriod(offset);
    }

    /// <summary>
    /// Get offset period
    /// </summary>
    /// <param name="moment">A moment within the payrun period</param>
    /// <param name="offset">The period offset count</param>
    /// <returns>The offset period</returns>
    public DatePeriod GetOffsetPeriod(DateTime moment, int offset)
    {
        if (!moment.IsUtc())
        {
            throw new ArgumentException("Value date must be UTC", nameof(moment));
        }
        var payrollPeriod = PayrollCalculator.GetPayrunPeriod(moment);
        return payrollPeriod.GetOffsetDatePeriod(offset);
    }

    private readonly Stack<DatePeriod> evaluationPeriods = new();

    /// <summary>
    /// The current evaluation period
    /// </summary>
    public DatePeriod EvaluationPeriod => evaluationPeriods.Peek();

    /// <summary>
    /// Push the current evaluation period
    /// </summary>
    /// <param name="evaluationPeriod">The new evaluation period</param>
    public void PushEvaluationPeriod(DatePeriod evaluationPeriod)
    {
        //Log.Warning($"Push on count {evaluationPeriods.Count}: {evaluationPeriod}");
        if (evaluationPeriods.Count == 0)
        {
            throw new InvalidOperationException();
        }
        if (!evaluationPeriod.IsUtc)
        {
            throw new ArgumentException("Value date must be UTC", nameof(evaluationPeriod));
        }
        evaluationPeriods.Push(evaluationPeriod);
    }

    /// <summary>
    /// Pop the current evaluation period
    /// </summary>
    /// <param name="evaluationPeriod">The new evaluation period</param>
    public void PopEvaluationPeriod(DatePeriod evaluationPeriod)
    {
        //Log.Warning($"Pop on count {evaluationPeriods.Count}: {evaluationPeriod}");
        if (evaluationPeriod == null)
        {
            throw new ArgumentNullException(nameof(evaluationPeriod));
        }
        if (!evaluationPeriod.IsUtc)
        {
            throw new ArgumentException("Value date must be UTC", nameof(evaluationPeriod));
        }
        if (evaluationPeriods.Count == 1)
        {
            throw new InvalidOperationException();
        }

        if (evaluationPeriod != evaluationPeriods.Peek())
        {
            throw new ArgumentException("Unbalanced stack operation on evaluation periods",
                nameof(evaluationPeriod));
        }

        evaluationPeriods.Pop();
    }

    #endregion

    #region Case Value

    /// <summary>
    /// Get all case slots from a specific case field
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case values</returns>
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
            throw new PayrollException($"Unknown case field {caseFieldName}");
        }

        // case value
        var caseValueRepository = GetCaseValueRepository(caseType.Value);
        return await caseValueRepository.GetCaseValueSlotsAsync(caseFieldName);
    }

    /// <summary>
    /// Get all case values (only active objects) by case type
    /// </summary>
    /// <param name="valueDate">The value date</param>
    /// <param name="caseType">The case type</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType)
    {
        if (!valueDate.IsUtc())
        {
            throw new ArgumentException("Value date must be UTC", nameof(valueDate));
        }
        var caseFields = (await CaseFieldProvider.CaseFieldRepository.GetDerivedCaseFieldsAsync(Settings.DbContext, caseType)).ToList();
        if (!caseFields.Any())
        {
            return new();
        }
        return await GetTimeCaseValuesAsync(valueDate, caseType, caseFields.Select(x => x.Name));
    }

    /// <summary>
    /// Get case values (only active objects) from a specific time
    /// </summary>
    /// <param name="valueDate">The value date</param>
    /// <param name="caseType">The case type</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    public async Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType,
        IEnumerable<string> caseFieldNames)
    {
        if (!valueDate.IsUtc())
        {
            throw new ArgumentException("Value date must be UTC", nameof(valueDate));
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
                    caseValue = caseFieldValues.Where(x => x.Start.HasValue && x.Start < valueDate).MaxBy(x => x.Start);
                    break;
                case CaseFieldTimeType.Period:
                case CaseFieldTimeType.ScaledPeriod:
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

    /// <summary>
    /// Get case values (only active objects)
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="evaluationPeriod">The evaluation period</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The case value periods for a time period</returns>
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
            throw new ArgumentException("Value date must be UTC", nameof(evaluationPeriod));
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

    /// <summary>
    /// Get case value by split periods
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseType">The case type</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>case values with value periods</returns>
    public async Task<IDictionary<CaseValue, List<DatePeriod>>> GetCaseValueSplitPeriodsAsync(
        string caseFieldName, CaseType caseType, string caseSlot = null)
    {
        ICaseValueCache caseValueRepository = GetCaseValueRepository(caseType);
        var caseValues = (await caseValueRepository.GetCaseValuesAsync(
            CaseValueReference.ToReference(caseFieldName, caseSlot))).ToList();
        if (caseValues.Any())
        {
            var calculator = new CaseValueProviderCalculation(PayrollCalculator, EvaluationDate, EvaluationPeriod);
            return calculator.SplitCaseValuePeriods(caseValues);
        }
        return new Dictionary<CaseValue, List<DatePeriod>>();
    }

    private async Task<CaseValue> GetMomentCaseValueAsync(string caseFieldName, CaseField caseField,
        ICaseValueCache caseValueRepository, DateTime moment, string caseSlot = null)
    {
        if (caseField == null)
        {
            throw new ArgumentNullException(nameof(caseField));
        }
        if (!moment.IsUtc())
        {
            throw new ArgumentException("Value date must be UTC", nameof(moment));
        }

        // case values (anytime)
        var caseValues = (await caseValueRepository.GetCaseValuesAsync(
            CaseValueReference.ToReference(caseFieldName, caseSlot))).ToList();
        if (!caseValues.Any())
        {
            return null;
        }

        // timeless value
        if (caseField.TimeType == CaseFieldTimeType.Timeless)
        {
            return CalculateTimelessCaseValue(caseValues);
        }

        // moment value
        return await CalculateMomentCaseValueAsync(caseValueRepository, caseField.Name, caseValues, moment);
    }

    private async Task<object> GetMomentValueAsync(string caseFieldName, CaseField caseField,
        ICaseValueCache caseValueRepository, DateTime moment, string caseSlot = null)
    {
        // case value
        var caseValue = await GetMomentCaseValueAsync(caseFieldName, caseField, caseValueRepository, moment, caseSlot);
        return caseValue?.GetValue();
    }

    /// <summary>
    /// Get case field period value
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseType">The case type</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The case value</returns>
    public async Task<object> GetPeriodValueAsync(string caseFieldName, CaseType? caseType, string caseSlot = null)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        caseType ??= await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);

        // ReSharper disable once PossibleInvalidOperationException
        var caseValueRepository = GetCaseValueRepository(caseType.Value);

        // case field
        var caseField = await CaseFieldProvider.GetValueCaseFieldAsync(Settings.DbContext, caseFieldName);
        if (caseField == null)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}");
        }

        // timeless case field
        if (caseField.TimeType == CaseFieldTimeType.Timeless)
        {
            return await GetMomentValueAsync(caseFieldName, caseField, caseValueRepository, EvaluationDate, caseSlot);
        }

        // case values
        var caseValues = (await CalculatePeriodCaseValuesAsync(caseValueRepository, caseField.Name)).ToList();
        if (!caseValues.Any())
        {
            return null;
        }

        // value calculation
        object value;
        var calculator = new CaseValueProviderCalculation(PayrollCalculator, EvaluationDate, EvaluationPeriod);
        switch (caseField.TimeType)
        {
            // timeless is handled before
            case CaseFieldTimeType.Moment:
                value = calculator.CalculatePeriodValue(caseField, caseValues);
                break;
            case CaseFieldTimeType.Period:
            case CaseFieldTimeType.ScaledPeriod:
                value = calculator.CalculateTimePeriodValue(caseField, caseValues);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown case value time type {caseField.TimeType}");
        }

        return value;
    }

    #endregion

    #region Period Values

    /// <summary>
    /// Get case period values by date period and the case field names
    /// </summary>
    /// <param name="period">The date period</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The case values for all case fields</returns>
    public async Task<IList<CaseFieldValue>> GetCasePeriodValuesAsync(
        DatePeriod period, IEnumerable<string> caseFieldNames)
    {
        if (caseFieldNames == null)
        {
            throw new ArgumentException(nameof(caseFieldNames));
        }
        if (!period.IsUtc)
        {
            throw new ArgumentException("Value date must be UTC", nameof(period));
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
                valueMoments.Add(periodStart.Value);
            }

            var periodEnd = casePeriodValue.End;
            if (periodEnd.HasValue && periodEnd.Value > period.Start && periodEnd.Value < period.End &&
                !valueMoments.Contains(periodEnd.Value))
            {
                valueMoments.Add(periodEnd.Value);
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
    public async Task<IList<CaseFieldValue>> GetCasePeriodValuesAsync(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        var caseType = await CaseFieldProvider.GetCaseTypeAsync(Settings.DbContext, caseFieldName);
        if (!caseType.HasValue)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}");
        }
        // ReSharper disable once PossibleInvalidOperationException
        ICaseValueCache caseValueRepository = GetCaseValueRepository(caseType.Value);
        if (caseValueRepository == null)
        {
            throw new ArgumentNullException(nameof(caseValueRepository));
        }

        // case field
        var values = new List<CaseFieldValue>();
        var caseField = await CaseFieldProvider.GetValueCaseFieldAsync(Settings.DbContext, caseFieldName);
        if (caseField == null)
        {
            throw new PayrollException($"Unknown case field {caseFieldName}");
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

        var calcType = caseField.TimeType;
        // period values with split periods
        switch (calcType)
        {
            case CaseFieldTimeType.Period:
            /*
            // only latest case value period values
            var newest = caseValues.OrderByDescending(x => x.Created).First();
            if (valuePeriods.ContainsKey(newest))
            {
                foreach (var datePeriod in valuePeriods[newest])
                {
                    values.Add(new()
                    {
                        CaseFieldName = caseFieldName,
                        CaseFieldNameLocalizations = caseField.NameLocalizations,
                        Created = newest.Created,
                        Start = datePeriod.Start,
                        End = datePeriod.End,
                        ValueType = caseField.ValueType,
                        Value = ValueConvert.ToJson(newest.GetValue())
                    });
                }
            }
            return values;
            */
            case CaseFieldTimeType.Moment:
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
                return values;
            case CaseFieldTimeType.ScaledPeriod:
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

                return values;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
    /// Determine the case value at a specific time
    /// </summary>
    private async Task<CaseValue> CalculateMomentCaseValueAsync(ICaseValueCache caseValueRepository,
        string caseFieldName, IEnumerable<CaseValue> caseValues, DateTime moment)
    {
        var caseValue = caseValues
            // values created before the moment and moment is within the evaluation period
            .Where(cv => cv.Created <= moment && cv.IsWithing(moment)).MaxBy(cv => cv.Created);

        await UpdateRetroCaseValue(caseValueRepository, caseFieldName);
        return caseValue;
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

    /// <summary>
    /// Gets the retro case value
    /// </summary>
    public CaseValue RetroCaseValue { get; private set; }

    /// <summary>
    /// Resets the retro case value
    /// </summary>
    public void ResetRetroCaseValue() => RetroCaseValue = null;

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