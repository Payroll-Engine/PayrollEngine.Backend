using System;

namespace PayrollEngine.Domain.Model;

/// <summary>Payroll month period</summary>
public class MonthPayrollPeriod : IPayrollPeriod
{
    private DatePeriod Period { get; }

    /// <summary>The payroll calendar</summary>
    public IPayrollCalendar Calendar { get; }

    /// <inheritdoc />
    public MonthPayrollPeriod(IPayrollCalendar calendar, DateTime moment) :
        this(calendar, moment.Year, moment.Month)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="MonthPayrollPeriod"/> class</summary>
    /// <param name="calendar">The calendar</param>
    /// <param name="year">The year</param>
    /// <param name="month">The month</param>
    public MonthPayrollPeriod(IPayrollCalendar calendar, int year, int month)
    {
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        Period = new(
            new(year, month, 1, 0, 0, 0, 0, calendar.Calendar, DateTimeKind.Utc),
            new DateTime(year, month, Date.DaysInMonth(year, month), 0, 0, 0, 0, calendar.Calendar, DateTimeKind.Utc).LastMomentOfDay());
    }

    #region IPayrollPeriod

    /// <inheritdoc />
    public DateTime Start => Period.Start;

    /// <inheritdoc />
    public DateTime End => Period.End;

    /// <inheritdoc />
    public string Name =>
        // ReSharper disable once StringLiteralTypo
        Period.Start.ToString("yyyy-MM", Calendar.Culture);

    /// <inheritdoc />
    public IPayrollPeriod GetPayrollPeriod(DateTime moment, int offset = 0) =>
        offset == 0 ? new(Calendar, moment) :
            new MonthPayrollPeriod(Calendar, moment.AddMonths(offset));

    #endregion

    /// <inheritdoc />
    public override string ToString() => Name;
}