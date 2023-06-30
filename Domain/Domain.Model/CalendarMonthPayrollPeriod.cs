using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll calendar month payroll period</summary>
public class CalendarMonthPayrollPeriod : IPayrollPeriod
{
    private DatePeriod Period { get; }

    /// <summary>
    /// The culture
    /// </summary>
    private CultureInfo Culture { get; }

    /// <summary>
    /// The date calendar
    /// </summary>
    private Calendar Calendar { get; }

    /// <inheritdoc />
    public CalendarMonthPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment) :
        this(culture, calendar, moment.Year, moment.Month)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CalendarMonthPayrollPeriod"/> class</summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    private CalendarMonthPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month)
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

        // calendar month start
        var monthStart = Culture.Calendar.GetOffsetDate(year, month);
        // calendar month end
        var monthEnd = Culture.Calendar.GetOffsetDate(year, month, offsetMonths: 1)
            .AddDays(-1).LastMomentOfDay();

        Period = new(monthStart, monthEnd);
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    public string Name =>
        Period.Start.ToString("yyyy-MM", Culture);

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Culture, Calendar, moment) :
            new CalendarMonthPayrollPeriod(Culture, Calendar, moment.AddMonths(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}