using System;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll week cycle</summary>
public class WeekPayrollCycle : IPayrollPeriod
{
    private DatePeriod Period { get; }

    /// <summary>The payroll calendar</summary>
    public IPayrollCalendar Calendar { get; }

    /// <summary>Gets the week of year</summary>
    public int WeekOfYear =>
        Calendar.GetWeekOfYear(Period.Start);

    /// <inheritdoc />
    public WeekPayrollCycle(IPayrollCalendar calendar, int year, int month, int day) :
        this(calendar, new(year, month, day, 0, 0, 0, DateTimeKind.Utc))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WeekPayrollCycle"/> class</summary>
    /// <param name="calendar">The calendar</param>
    /// <param name="moment">The moment</param>
    public WeekPayrollCycle(IPayrollCalendar calendar, DateTime moment)
    {
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        var startOfWeek = moment.GetPreviousWeekDay(calendar.Configuration.FirstDayOfWeek);

        // TODO: calculate start cycle for week periods
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
        Period.Start.ToString("yyyy", Calendar.Culture);

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Calendar, moment) :
            new WeekPayrollCycle(Calendar, moment.AddYears(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}