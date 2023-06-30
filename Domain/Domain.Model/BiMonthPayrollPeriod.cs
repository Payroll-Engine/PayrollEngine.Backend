using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll two month payroll period</summary>
public class BiMonthPayrollPeriod : IPayrollPeriod
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
    public BiMonthPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment) :
        this(culture, calendar, moment.Year, moment.Month)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BiMonthPayrollPeriod"/> class</summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    private BiMonthPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month)
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

        const int monthsInBiMonths = 2;

        // month offset
        var firstMonthOfYear = (int)(calendar.FirstMonthOfYear ?? Month.January);
        var yearOffset = (firstMonthOfYear - month) % monthsInBiMonths;
        var offsetMonths = month >= firstMonthOfYear ?
            // next year
            yearOffset :
            // previous year
            yearOffset * -1;

        // bi month start
        var biMonthStart = Culture.Calendar.GetOffsetDate(year, month, offsetMonths: offsetMonths);
        // bi month end
        var biMonthEnd = Culture.Calendar.GetOffsetDate(year, month, offsetMonths: offsetMonths + monthsInBiMonths)
            .AddDays(-1).LastMomentOfDay();

        Period = new(biMonthStart, biMonthEnd);
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
            new BiMonthPayrollPeriod(Culture, Calendar, moment.AddMonths(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}