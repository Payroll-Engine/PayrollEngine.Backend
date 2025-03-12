using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll week period
/// </summary>
public class WeekPayrollPeriod : IPayrollPeriod
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

    /// <summary>
    /// Gets the week of year
    /// </summary>
    private int WeekOfYear =>
        Calendar.GetWeekOfYear(Culture, Period.Start);

    /// <inheritdoc />
    public WeekPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month, int day) :
        this(culture, calendar, new(year, month, day, 0, 0, 0, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeekPayrollPeriod"/> class
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="moment">The moment</param>
    public WeekPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment)
    {
        Culture = culture ?? throw new ArgumentNullException(nameof(culture));
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));

        var firstDayOfWeek = calendar.FirstDayOfWeek ?? (DayOfWeek)culture.DateTimeFormat.FirstDayOfWeek;
        var startOfWeek = moment.GetPreviousWeekDay(firstDayOfWeek).Date;
        Period = new(
            startOfWeek,
            startOfWeek.AddDays(Date.DaysInWeek - 1).LastMomentOfDay());
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    public string Name =>
        $"{Period.Start.ToString("yyyy", Culture)} w{WeekOfYear}";

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Culture, Calendar, moment) :
            new WeekPayrollPeriod(Culture, Calendar, moment.AddDays(offset * Date.DaysInWeek));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}