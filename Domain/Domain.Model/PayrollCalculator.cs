using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <inheritdoc />
public class PayrollCalculator : IPayrollCalculator
{
    /// <summary>The culture</summary>
    public CultureInfo Culture { get; }

    /// <summary>The payroll calendar</summary>
    public Calendar Calendar { get; }

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
    public virtual IPayrollPeriod GetPayrunCycle(DateTime cycleMoment) =>
        GetPayrunPeriod(cycleMoment, Calendar.CycleTimeUnit);

    /// <inheritdoc />
    public virtual IPayrollPeriod GetPayrunPeriod(DateTime periodMoment) =>
        GetPayrunPeriod(periodMoment, Calendar.PeriodTimeUnit);

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
    public virtual decimal? CalculateCasePeriodValue(CaseValueCalculation calculation)
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
    protected virtual decimal? CalculateYearValue(CaseValueCalculation calculation)
    {
        // year period
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var period = new DatePeriod(calculation.CaseValuePeriod.Start.Date, periodEnd);

        // year week days
        var yearDayCount = GetPeriodWeekDayCount(calculation, (decimal)period.Duration.TotalDays);
        if (yearDayCount <= 0)
        {
            return null;
        }

        // total days in year
        var yearTotalDayCount = GetPeriodDayCount(period);
        if (yearTotalDayCount <= 0)
        {
            return null;
        }

        // case value (no difference between cycle and period value)
        var caseValue = calculation.CaseValue;

        // year value: scale base value with the day factor
        var yearValue = caseValue / yearTotalDayCount * yearDayCount;
        return yearValue;
    }

    /// <summary>
    /// Calculate the semi year period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case semi year period value</returns>
    protected virtual decimal? CalculateSemiYearValue(CaseValueCalculation calculation)
    {
        // semi year period
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var period = new DatePeriod(calculation.CaseValuePeriod.Start.Date, periodEnd);

        // semi year week days
        var semiYearDayCount = GetPeriodWeekDayCount(calculation, (decimal)period.Duration.TotalDays);
        if (semiYearDayCount <= 0)
        {
            return null;
        }

        // total days in semi year
        var semiYearTotalDayCount = GetPeriodDayCount(period);
        if (semiYearTotalDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.SemiYearsInYear);

        // semi year value: scale base value with the day factor
        var semiYearValue = caseValue / semiYearTotalDayCount * semiYearDayCount;
        return semiYearValue;
    }

    /// <summary>
    /// Calculate the quarter period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case quarter period value</returns>
    protected virtual decimal? CalculateQuarterValue(CaseValueCalculation calculation)
    {
        // quarter period
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var period = new DatePeriod(calculation.CaseValuePeriod.Start.Date, periodEnd);

        // quarter week days
        var quarterDayCount = GetPeriodWeekDayCount(calculation, (decimal)period.Duration.TotalDays);
        if (quarterDayCount <= 0)
        {
            return null;
        }

        // total days in quarter
        var quarterTotalDayCount = GetPeriodDayCount(period);
        if (quarterTotalDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.QuartersInYear);

        // quarter value: scale base value with the day factor
        var quarterValue = caseValue / quarterTotalDayCount * quarterDayCount;
        return quarterValue;
    }

    /// <summary>
    /// Calculate the bi month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case bi month period value</returns>
    protected virtual decimal? CalculateBiMonthValue(CaseValueCalculation calculation)
    {
        // bi month period
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var period = new DatePeriod(calculation.CaseValuePeriod.Start.Date, periodEnd);

        // bi month week days
        var biMonthDayCount = GetPeriodWeekDayCount(calculation, (decimal)period.Duration.TotalDays);
        if (biMonthDayCount <= 0)
        {
            return null;
        }

        // total days in bi month
        var biMonthTotalDayCount = GetPeriodDayCount(period);
        if (biMonthTotalDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.BiMonthsInYear);

        // bi month value: scale base value with the day factor
        var biMonthValue = caseValue / biMonthTotalDayCount * biMonthDayCount;
        return biMonthValue;
    }

    /// <summary>
    /// Calculate the calendar month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case calendar month period value</returns>
    protected virtual decimal? CalculateCalendarMonthValue(CaseValueCalculation calculation)
    {
        if (!calculation.EvaluationPeriod.Start.IsSameMonth(calculation.EvaluationPeriod.End))
        {
            throw new PayrollException($"Evaluation period {calculation.EvaluationPeriod} must be within a month");
        }

        // calendar month period
        var periodStart = calculation.CaseValuePeriod.Start;
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var period = new DatePeriod(periodStart.Date, periodEnd);

        // calendar month days
        var monthDayCount = GetPeriodWeekDayCount(calculation, (decimal)period.Duration.TotalDays);
        if (monthDayCount <= 0)
        {
            return null;
        }

        // total days in month
        var monthTotalDayCount = (decimal)DateTime.DaysInMonth(periodStart.Year, periodStart.Month);
        if (Calendar.MonthDayCount.HasValue)
        {
            monthTotalDayCount = Calendar.MonthDayCount.Value;
        }
        if (monthTotalDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.MonthsInYear);

        // calendar month value: scale base value with the day factor
        var monthValue = caseValue / monthTotalDayCount * monthDayCount;
        return monthValue;
    }

    /// <summary>
    /// Calculate the semi month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case semi month period value</returns>
    protected virtual decimal? CalculateLunisoralMonthValue(CaseValueCalculation calculation)
    {
        // lunar month period
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var period = new DatePeriod(calculation.CaseValuePeriod.Start.Date, periodEnd);

        // lunar calendar month days
        var monthDayCount = GetPeriodWeekDayCount(calculation, (decimal)period.Duration.TotalDays);
        if (monthDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.LunisolarMonthsInYear);

        // lunisolar month value: scale base value with the day factor
        var monthValue = caseValue / Date.DaysInLunisolarMonth * monthDayCount;
        return monthValue;
    }

    /// <summary>
    /// Calculate the semi month period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case semi month period value</returns>
    protected virtual decimal? CalculateSemiMonthValue(CaseValueCalculation calculation)
    {
        // semi month period
        var periodStart = calculation.CaseValuePeriod.Start;
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var period = new DatePeriod(periodStart.Date, periodEnd);

        // semi month day count
        var semiMonthDayCount = GetPeriodWeekDayCount(calculation, (decimal)period.Duration.TotalDays);
        if (semiMonthDayCount <= 0)
        {
            return null;
        }

        // total semi month days
        var daysInMonth = DateTime.DaysInMonth(periodStart.Year, periodStart.Month);
        var secondHalfStartDay = Date.DaysInSemiMonth + 1;
        var semiMonthTotalDayCount = periodStart.Day < secondHalfStartDay ?
            // first half: always 15
            Date.DaysInSemiMonth :
            // second half: 13/14 (february) or 15 (months with 30 days) or 16 (months with 31 days)
            daysInMonth - Date.DaysInSemiMonth;
        if (semiMonthTotalDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.SemiMonthsInYear);

        // semi month value: scale base value with the day factor
        var monthValue = caseValue / semiMonthTotalDayCount * semiMonthDayCount;
        return monthValue;
    }

    /// <summary>
    /// Calculate the bi week period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case bi week period value</returns>
    protected virtual decimal? CalculateBiWeekValue(CaseValueCalculation calculation)
    {
        // bi week days
        var biWeekDayCount = GetPeriodWeekDayCount(calculation, Date.DaysInBiWeek);
        if (biWeekDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.BiWeeksInYear);

        // bi week value: scale base value with the day factor
        var biWeekValue = caseValue / Date.DaysInBiWeek * biWeekDayCount;
        return biWeekValue;
    }

    /// <summary>
    /// Calculate the week period value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case week period value</returns>
    protected virtual decimal? CalculateWeekValue(CaseValueCalculation calculation)
    {
        // week days
        var weekDayCount = GetPeriodWeekDayCount(calculation, Date.DaysInWeek);
        if (weekDayCount <= 0)
        {
            return null;
        }

        // case value
        var caseValue = GetScaledPeriodValue(calculation, Date.WeeksInYear);

        // week value: scale base value with the day factor
        var weekValue = caseValue / Date.DaysInWeek * weekDayCount;
        return weekValue;
    }

    /// <summary>
    /// Get the week day count form the case value period
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <param name="weekCount">The week mode day count</param>
    protected virtual decimal GetPeriodWeekDayCount(CaseValueCalculation calculation, decimal weekCount)
    {
        var weekDayCount = 0m;
        switch (Calendar.WeekMode)
        {
            case CalendarWeekMode.Week:
                weekDayCount = weekCount;
                break;
            case CalendarWeekMode.WorkWeek:
                weekDayCount = calculation.CaseValuePeriod.GetWorkingDaysCount(Calendar.GetWorkDays());
                break;
        }
        return weekDayCount;
    }

    /// <summary>
    /// Get the period day count
    /// </summary>
    /// <param name="period">The period to count</param>
    /// <returns></returns>
    protected virtual decimal GetPeriodDayCount(DatePeriod period)
    {
        var daysInPeriod = 0m;
        var month = period.Start.Date;
        while (month < period.End)
        {
            var daysInMonth = (decimal)Date.DaysInMonth(month);
            daysInPeriod += daysInMonth;
            month = month.AddMonths(1);
        }
        return daysInPeriod;
    }

    /// <summary>
    /// Scale the period case value
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <param name="cycleFactor">The cycle factor</param>
    protected virtual decimal GetScaledPeriodValue(CaseValueCalculation calculation, decimal cycleFactor)
    {
        var caseValue = Calendar.TimeMap switch
        {
            CalendarTimeMap.Cycle => calculation.CaseValue / cycleFactor,
            CalendarTimeMap.Period => calculation.CaseValue,
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
