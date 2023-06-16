using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll quarter payroll period</summary>
public class QuarterPayrollPeriod : IPayrollPeriod
{
    private DatePeriod Period { get; }

    /// <summary>
    /// The culture
    /// </summary>
    public CultureInfo Culture { get; }

    /// <summary>
    /// The date calendar
    /// </summary>
    public Calendar Calendar { get; }

    /// <inheritdoc />
    public QuarterPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment) :
        this(culture, calendar, moment.Year, moment.Month)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="QuarterPayrollPeriod"/> class</summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    public QuarterPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month)
    {
        // arguments
        Culture = culture ?? throw new ArgumentNullException(nameof(culture));
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month));
        }

        // month offset
        var firstMonthOfYear = (int)(
            calendar.FirstMonthOfYear ?? Month.January);
        var quarterOffset = (firstMonthOfYear - month) % Date.MonthsInQuarter;
        var offsetMonths = month >= firstMonthOfYear ?
            // next year
            quarterOffset :
            // previous year
            (Date.MonthsInQuarter - quarterOffset) * -1;

        // quarter start
        var quarterStart = Culture.Calendar.GetOffsetDate(year, month, offsetMonths: offsetMonths);
        // quarter end
        var quarterEnd = Culture.Calendar.GetOffsetDate(year, month, offsetMonths: offsetMonths + Date.MonthsInQuarter)
            .AddDays(-1).LastMomentOfDay();

        Period = new(quarterStart, quarterEnd);
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    public string Name =>
        $"{Period.Start.ToString("yyyy-MM", Culture)} - {Period.End.ToString("yyyy-MM", Culture)}";

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Culture, Calendar, moment) :
            new QuarterPayrollPeriod(Culture, Calendar, moment.AddMonths(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}