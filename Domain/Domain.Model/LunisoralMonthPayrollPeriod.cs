using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll lunisolar month payroll period</summary>
public class LunisoralMonthPayrollPeriod : IPayrollPeriod
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
    public LunisoralMonthPayrollPeriod(CultureInfo culture, Calendar calendar, int year, int month, int day) :
        this(culture, calendar, new(year, month, day, 0, 0, 0, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunisoralMonthPayrollPeriod"/> class
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <param name="calendar">The calendar</param>
    /// <param name="moment">The moment</param>
    public LunisoralMonthPayrollPeriod(CultureInfo culture, Calendar calendar, DateTime moment)
    {
        // arguments
        Culture = culture ?? throw new ArgumentNullException(nameof(culture));
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));

        // lunisolar month first week
        var firstDayOfWeek = calendar.FirstDayOfWeek ?? (DayOfWeek)culture.DateTimeFormat.FirstDayOfWeek;
        var startOfWeek = moment.GetPreviousWeekDay(firstDayOfWeek).Date;
        var startWeekOfYear = Calendar.GetWeekOfYear(Culture, startOfWeek);
        if (startWeekOfYear > 1)
        {
            var weekOffset = (startWeekOfYear - 1) % Date.WeeksInLunisolarMonth;
            // moment within 2nd and 4th week
            if (weekOffset != 0)
            {
                startOfWeek = startOfWeek.AddDays(Date.DaysInWeek * weekOffset * -1);
            }
        }

        Period = new(
            startOfWeek,
            startOfWeek.AddDays(Date.DaysInLunisolarMonth - 1).LastMomentOfDay());
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
            new LunisoralMonthPayrollPeriod(Culture, Calendar, moment.AddMonths(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}