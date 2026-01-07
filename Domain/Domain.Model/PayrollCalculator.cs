using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <inheritdoc />
public class PayrollCalculator : IPayrollCalculator
{
    /// <summary>The culture</summary>
    private CultureInfo Culture { get; }

    /// <summary>The payroll calendar</summary>
    private Calendar Calendar { get; }

    /// <summary>Initializes a new instance of the <see cref="PayrollCalculator"/> class, using the current culture and default calendar</summary>
    public PayrollCalculator() :
        this(CultureInfo.CurrentCulture, new())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PayrollCalculator"/> class</summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The payroll calendar</param>
    public PayrollCalculator(CultureInfo culture = null, Calendar calendar = null)
    {
        Culture = culture ?? CultureInfo.CurrentCulture;
        Calendar = calendar ?? new();
    }

    /// <inheritdoc />
    public CalendarTimeUnit CycleTimeUnit => Calendar.CycleTimeUnit;

    /// <inheritdoc />
    public CalendarTimeUnit PeriodTimeUnit => Calendar.PeriodTimeUnit;

    /// <inheritdoc />
    public IPayrollPeriod GetPayrunCycle(DateTime cycleMoment) =>
        GetPayrunPeriod(cycleMoment, Calendar.CycleTimeUnit);

    /// <inheritdoc />
    public IPayrollPeriod GetPayrunPeriod(DateTime periodMoment) =>
        GetPayrunPeriod(periodMoment, Calendar.PeriodTimeUnit);

    /// <inheritdoc />
    public int GetCalendarDayCount(DatePeriod period)
    {
        var dayCount = 0m;
        switch (Calendar.WeekMode)
        {
            case CalendarWeekMode.Week:
                dayCount = (decimal)period.Duration.TotalDays;
                break;
            case CalendarWeekMode.WorkWeek:
                dayCount = period.GetWorkingDaysCount(Calendar.GetWeekDays());
                break;
        }
        return Convert.ToInt32(dayCount);
    }

    private IPayrollPeriod GetPayrunPeriod(DateTime periodMoment, CalendarTimeUnit timeUnit)
    {
        return timeUnit switch
        {
            CalendarTimeUnit.Year => new YearPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.SemiYear => new SemiYearPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.Quarter => new QuarterPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.BiMonth => new BiMonthPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.CalendarMonth => new CalendarMonthPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.LunisolarMonth => new LunisoralMonthPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.SemiMonth => new SemiMonthPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.BiWeek => new BiWeekPayrollPeriod(Culture, Calendar, periodMoment),
            CalendarTimeUnit.Week => new WeekPayrollPeriod(Culture, Calendar, periodMoment),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <inheritdoc />
    public decimal? CalculateCasePeriodValue(CaseValueCalculation calculation)
    {
        switch (Calendar.PeriodTimeUnit)
        {
            case CalendarTimeUnit.Year:
                return CalculateYearValue(calculation);
            case CalendarTimeUnit.SemiYear:
                return CalculateSemiYearValue(calculation);
            case CalendarTimeUnit.Quarter:
                return CalculateQuarterValue(calculation);
            case CalendarTimeUnit.BiMonth:
                return CalculateBiMonthValue(calculation);
            case CalendarTimeUnit.CalendarMonth:
                return CalculateCalendarMonthValue(calculation);
            case CalendarTimeUnit.LunisolarMonth:
                return CalculateLunisoralMonthValue(calculation);
            case CalendarTimeUnit.SemiMonth:
                return CalculateSemiMonthValue(calculation);
            case CalendarTimeUnit.BiWeek:
                return CalculateBiWeekValue(calculation);
            case CalendarTimeUnit.Week:
                return CalculateWeekValue(calculation);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Calculate the year period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case year period value</returns>
    private decimal? CalculateYearValue(CaseValueCalculation calculation)
    {
        // year week days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in year
        var evaluationDayCount = GetEvaluationDayCount(calculation);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value (no difference between cycle and period value)
        var caseValue = calculation.CaseValue;

        // year value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the semi year period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case semi year period value</returns>
    private decimal? CalculateSemiYearValue(CaseValueCalculation calculation)
    {
        // semi year week days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in semi year
        var evaluationDayCount = GetEvaluationDayCount(calculation);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.SemiYearsInYear);

        // semi year value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the quarter period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case quarter period value</returns>
    private decimal? CalculateQuarterValue(CaseValueCalculation calculation)
    {
        // quarter week days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in quarter
        var evaluationDayCount = GetEvaluationDayCount(calculation);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.QuartersInYear);

        // quarter value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the bi month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case bi month period value</returns>
    private decimal? CalculateBiMonthValue(CaseValueCalculation calculation)
    {
        // bi month week days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in bi month
        var evaluationDayCount = GetEvaluationDayCount(calculation);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.BiMonthsInYear);

        // bi month value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the calendar month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case calendar month period value</returns>
    private decimal? CalculateCalendarMonthValue(CaseValueCalculation calculation)
    {
        if (!calculation.EvaluationPeriod.Start.IsSameMonth(calculation.EvaluationPeriod.End))
        {
            throw new PayrollException($"Evaluation period {calculation.EvaluationPeriod} must be within a month.");
        }

        // calendar month days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in month
        var periodStart = calculation.CaseValuePeriod.Start;
        var evaluationDayCount = GetEvaluationDayCount(calculation, DateTime.DaysInMonth(periodStart.Year, periodStart.Month));
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.MonthsInYear);

        // calendar month value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the lunisolar month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case semi month period value</returns>
    private decimal? CalculateLunisoralMonthValue(CaseValueCalculation calculation)
    {
        // lunar calendar month days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in lunisolar month
        var evaluationDayCount = GetEvaluationDayCount(calculation, Date.DaysInLunisolarMonth);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.LunisolarMonthsInYear);

        // lunisolar month value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the semi month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case semi month period value</returns>
    private decimal? CalculateSemiMonthValue(CaseValueCalculation calculation)
    {
        // semi month day count
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total semi month days
        var evaluationDayCount = GetEvaluationDayCount(calculation);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.SemiMonthsInYear);

        // semi month value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the bi-week period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case bi-week period value</returns>
    private decimal? CalculateBiWeekValue(CaseValueCalculation calculation)
    {
        // bi-week days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in bi-week
        var evaluationDayCount = GetEvaluationDayCount(calculation, Date.DaysInBiWeek);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.BiWeeksInYear);

        // bi-week value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Calculate the week period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case week period value</returns>
    private decimal? CalculateWeekValue(CaseValueCalculation calculation)
    {
        // week days
        var valueDayCount = GetValueDayCount(calculation);
        if (valueDayCount <= 0)
        {
            return null;
        }

        // total days in week
        var evaluationDayCount = GetEvaluationDayCount(calculation, Date.DaysInWeek);
        if (evaluationDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = MapPeriodValue(calculation, Date.WeeksInYear);

        // week value: scale base value with the day factor
        var value = caseValue / evaluationDayCount * valueDayCount;
        return value.RoundPayroll();
    }

    /// <summary>
    /// Get the evaluation day count form the case value period
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <param name="evaluationDayCount">Evaluation day count</param>
    private decimal GetEvaluationDayCount(CaseValueCalculation calculation, decimal? evaluationDayCount = null)
    {
        // fixed period day count
        if (Calendar.PeriodDayCount != null)
        {
            return Calendar.PeriodDayCount.Value;
        }

        var dayCount = 0m;
        switch (Calendar.WeekMode)
        {
            case CalendarWeekMode.Week:
                if (evaluationDayCount.HasValue)
                {
                    dayCount = evaluationDayCount.Value;
                }
                else
                {
                    dayCount = (decimal)calculation.EvaluationPeriod.TotalDays;
                }
                break;
            case CalendarWeekMode.WorkWeek:
                dayCount = calculation.EvaluationPeriod.GetWorkingDaysCount(Calendar.GetWeekDays());
                break;
        }
        return dayCount;
    }

    /// <summary>
    /// Get the value count form the case value period
    /// </summary>
    /// <param name="calculation">The calculation</param>
    private decimal GetValueDayCount(CaseValueCalculation calculation)
    {
        var dayCount = 0m;
        switch (Calendar.WeekMode)
        {
            case CalendarWeekMode.Week:
                var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
                var period = new DatePeriod(calculation.CaseValuePeriod.Start.Date, periodEnd);
                dayCount = (decimal)period.Duration.TotalDays;
                break;
            case CalendarWeekMode.WorkWeek:
                dayCount = calculation.CaseValuePeriod.GetWorkingDaysCount(Calendar.GetWeekDays());
                break;
        }
        return dayCount;
    }

    /// <summary>
    /// Scale the period case value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <param name="cycleFactor">The cycle factor</param>
    private decimal MapPeriodValue(CaseValueCalculation calculation, decimal cycleFactor)
    {
        var caseValue = Calendar.TimeMap switch
        {
            CalendarTimeMap.Cycle => calculation.CaseValue / cycleFactor,
            _ => calculation.CaseValue
        };
        return caseValue;
    }

    /// <summary>
    /// The string representation
    /// </summary>
    public override string ToString() =>
        $"{Culture?.Name} {Calendar?.Name}";
}
