using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll semi year payroll period</summary>
public class SemiYearPayrollPeriod : IPayrollPeriod
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
    public SemiYearPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment) :
        this(culture, calendar, moment.Year, moment.Month)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemiYearPayrollPeriod"/> class
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    public SemiYearPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month)
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
        var semiYearOffset = (firstMonthOfYear - month) % Date.MonthsInSemiYear;
        var offsetMonths = month >= firstMonthOfYear ?
            // next year
            semiYearOffset :
            // previous year
            (Date.MonthsInSemiYear - semiYearOffset) * -1;

        // semi year start
        var semiYearStart = Culture.Calendar.GetOffsetDate(year, month, offsetMonths: offsetMonths);
        // semi year end
        var semiYearEnd = Culture.Calendar.GetOffsetDate(year, month, offsetMonths: offsetMonths + Date.MonthsInSemiYear)
            .AddDays(-1).LastMomentOfDay();

        Period = new(semiYearStart, semiYearEnd);
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    // ReSharper disable once StringLiteralTypo
    public string Name =>
        $"{Period.Start.ToString("yyyy.MM", Culture)} - {Period.End.ToString("yyyy.MM", Culture)}";

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Culture, Calendar, moment) :
            new SemiYearPayrollPeriod(Culture, Calendar, moment.AddYears(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}