using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll week period
/// </summary>
public class BiWeekPayrollPeriod : IPayrollPeriod
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
    /// Gets the first week of year
    /// </summary>
    private int StartWeekOfYear { get; }

    /// <summary>
    /// Gets the first week of year
    /// </summary>
    private int EndWeekOfYear { get; }

    /// <inheritdoc />
    public BiWeekPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month, int day) :
        this(culture, calendar, new(year, month, day, 0, 0, 0, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BiWeekPayrollPeriod"/> class
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="moment">The moment</param>
    public BiWeekPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment)
    {
        Culture = culture ?? throw new ArgumentNullException(nameof(culture));
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));

        // bi week first week
        var firstDayOfWeek = calendar.FirstDayOfWeek ?? (DayOfWeek)culture.DateTimeFormat.FirstDayOfWeek;
        var startOfWeek = moment.GetPreviousWeekDay(firstDayOfWeek).Date;
        var startWeekOfYear = Calendar.GetWeekOfYear(Culture, startOfWeek);
        if (startWeekOfYear > 1)
        {
            // moment within second week
            //if (startWeekOfYear % 2 == 0)
            //{
            //    startWeekOfYear--;
            //    startOfWeek = startOfWeek.AddDays(Date.DaysInWeek * -1);
            //}
            if ((startWeekOfYear - 1) % 2 == 1)
            {
                startWeekOfYear--;
                startOfWeek = startOfWeek.AddDays(Date.DaysInWeek * -1);
            }
        }

        Period = new(
            startOfWeek,
            startOfWeek.AddDays(Date.DaysInBiWeek - 1).LastMomentOfDay());

        StartWeekOfYear = startWeekOfYear;
        EndWeekOfYear = Calendar.GetWeekOfYear(Culture, Period.End);
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    public string Name =>
        $"{Period.Start.ToString("yyyy", Culture)} {StartWeekOfYear} - {Period.End.ToString("yyyy", Culture)} {EndWeekOfYear}";

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Culture, Calendar, moment) :
            new BiWeekPayrollPeriod(Culture, Calendar, moment.AddDays(offset * Date.DaysInWeek));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}