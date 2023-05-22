using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll year cycle
/// </summary>
public class YearPayrollCycle : IPayrollPeriod
{
    private DatePeriod Period { get; }

    /// <summary>
    /// The date calendar
    /// </summary>
    public IPayrollCalendar Calendar { get; }

    /// <inheritdoc />
    public YearPayrollCycle(IPayrollCalendar calendar, DateTime moment) :
        this(calendar, moment.Year, moment.Month)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YearPayrollCycle"/> class
    /// </summary>
    /// <param name="calendar">The calendar</param>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    public YearPayrollCycle(IPayrollCalendar calendar, int year, int month)
    {
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));

        // period
        var periodStart = new DateTime(year, month, 1, 0, 0, 0, 0, calendar.Calendar, DateTimeKind.Utc);

        // first moment of the year
        DateTime cycleStart;
        if (month == (int)calendar.Configuration.FirstMonthOfYear)
        {
            cycleStart = periodStart;
        }
        else if (month > (int)calendar.Configuration.FirstMonthOfYear)
        {
            cycleStart = periodStart.AddMonths((month - (int)calendar.Configuration.FirstMonthOfYear) * -1);
        }
        else
        {
            cycleStart = periodStart.AddMonths((month + Date.MonthsInYear - (int)calendar.Configuration.FirstMonthOfYear) * -1);
        }

        // last moment of the year
        var cycleEnd = cycleStart.AddMonths(Date.MonthsInYear).AddDays(-1).LastMomentOfDay();
        Period = new(cycleStart, cycleEnd);
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    // ReSharper disable once StringLiteralTypo
    public string Name =>
        Period.Start.ToString("yyyy", Calendar.Culture);

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Calendar, moment) :
            new YearPayrollCycle(Calendar, moment.AddYears(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}