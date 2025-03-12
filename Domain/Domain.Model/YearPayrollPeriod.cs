using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll year payroll period</summary>
public class YearPayrollPeriod : IPayrollPeriod
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
    public YearPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment) :
        this(culture, calendar, moment.Year, moment.Month)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YearPayrollPeriod"/> class
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    private YearPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month)
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
        var firstMonthOfYear = (int)(calendar.FirstMonthOfYear ?? Month.January);

        var yearOffset = (firstMonthOfYear - month) % Date.MonthsInYear;
        var offsetMonths = month >= firstMonthOfYear ?
            // next year
            yearOffset :
            // previous year
            (Date.MonthsInYear - yearOffset) * -1;

        // year-start
        var yearStart = Culture.Calendar.GetOffsetDate(year, month, offsetMonths);
        // year-end
        var yearEnd = Culture.Calendar.GetOffsetDate(year + 1, month, offsetMonths)
            .AddDays(-1).LastMomentOfDay();

        Period = new(yearStart, yearEnd);
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    // ReSharper disable once StringLiteralTypo
    public string Name =>
        Calendar.FirstMonthOfYear == Month.January ?
            Period.Start.ToString("yyyy", Culture) :
            $"{Period.Start.ToString("yyyy.MM", Culture)} - {Period.End.ToString("yyyy.MM", Culture)}";

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Culture, Calendar, moment) :
            new YearPayrollPeriod(Culture, Calendar, moment.AddYears(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}