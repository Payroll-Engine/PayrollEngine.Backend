using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll week period
/// </summary>
public class WeekPayrollPeriod : IPayrollPeriod
{
    private DatePeriod Period { get; }

    /// <summary>
    /// The date calendar
    /// </summary>
    public IPayrollCalendar Calendar { get; }

    /// <summary>
    /// Gets the week of year
    /// </summary>
    public int WeekOfYear =>
        Calendar.GetWeekOfYear(Period.Start);

    /// <inheritdoc />
    public WeekPayrollPeriod(IPayrollCalendar calendar, int year, int month, int day) :
        this(calendar, new(year, month, day, 0, 0, 0, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeekPayrollPeriod"/> class
    /// </summary>
    /// <param name="calendar">The calendar</param>
    /// <param name="moment">The moment</param>
    public WeekPayrollPeriod(IPayrollCalendar calendar, DateTime moment)
    {
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        var startOfWeek = moment.GetPreviousWeekDay(calendar.Configuration.FirstDayOfWeek);
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
        $"{Period.Start.ToString("yyyy", Calendar.Culture)} {WeekOfYear}";

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Calendar, moment) :
            new WeekPayrollPeriod(Calendar, moment.AddDays(offset * Date.DaysInWeek));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}