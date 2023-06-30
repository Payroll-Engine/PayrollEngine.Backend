using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll calendar
/// </summary>
public class Calendar : DomainObjectBase, INamedObject, IDomainAttributeObject, IEquatable<Calendar>
{
    /// <summary>
    /// The division name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized division names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The cycle time unit (default: year)
    /// </summary>
    public CalendarTimeUnit CycleTimeUnit { get; set; } = CalendarTimeUnit.Year;

    /// <summary>
    /// The period time unit (default: calendar month)
    /// </summary>
    public CalendarTimeUnit PeriodTimeUnit { get; set; } = CalendarTimeUnit.CalendarMonth;

    /// <summary>
    /// The time map (default: period)
    /// </summary>
    public CalendarTimeMap TimeMap { get; set; } = CalendarTimeMap.Period;

    /// <summary>The first month of a year  (default: january)</summary>
    public Month? FirstMonthOfYear { get; set; } = Month.January;

    /// <summary>Override the effective month day count</summary>
    public decimal? MonthDayCount { get; set; }

    /// <summary>Override the calendar year start week rule</summary>
    public CalendarWeekRule? YearWeekRule { get; set; }

    /// <summary>Override the calendar first day of week</summary>
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// The week mode (default: week)
    /// </summary>
    public CalendarWeekMode WeekMode { get; set; } = CalendarWeekMode.Week;

    /// <summary>Work on monday (default: true), used by <see cref="CalendarWeekMode.WorkWeek"/> </summary>
    public bool WorkMonday { get; set; } = true;

    /// <summary>Work on tuesday (default: true), used by <see cref="CalendarWeekMode.WorkWeek"/> </summary>
    public bool WorkTuesday { get; set; } = true;

    /// <summary>Work on wednesday (default: true), used by <see cref="CalendarWeekMode.WorkWeek"/> </summary>
    public bool WorkWednesday { get; set; } = true;

    /// <summary>Work on thursday (default: true), used by <see cref="CalendarWeekMode.WorkWeek"/> </summary>
    public bool WorkThursday { get; set; } = true;

    /// <summary>Work on friday (default: true), used by <see cref="CalendarWeekMode.WorkWeek"/> </summary>
    public bool WorkFriday { get; set; } = true;

    /// <summary>Work on saturday (default: false), used by <see cref="CalendarWeekMode.WorkWeek"/> </summary>
    public bool WorkSaturday { get; set; }

    /// <summary>Work on sunday (default: false), used by <see cref="CalendarWeekMode.WorkWeek"/> </summary>
    public bool WorkSunday { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public Calendar()
    {
    }

    /// <inheritdoc/>
    public Calendar(Calendar copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Calendar compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}