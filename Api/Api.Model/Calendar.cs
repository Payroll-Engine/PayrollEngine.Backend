using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Model;

/// <summary>
/// The division API object
/// </summary>
public class Calendar : ApiObjectBase
{
    /// <summary>The division name</summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>The localized division names</summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>The cycle time unit</summary>
    [Required]
    public CalendarTimeUnit CycleTimeUnit { get; set; }

    /// <summary>The period time unit</summary>
    [Required]
    public CalendarTimeUnit PeriodTimeUnit { get; set; }

    /// <summary>The time map</summary>
    public CalendarTimeMap TimeMap { get; set; }

    /// <summary>The first month of a year</summary>
    public Month? FirstMonthOfYear { get; set; }

    /// <summary>Override the effective month day count</summary>
    public decimal? MonthDayCount { get; set; }

    /// <summary>Override the calendar year start week rule</summary>
    public CalendarWeekRule? YearWeekRule { get; set; }

    /// <summary>Override the calendar first day of week</summary>
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>The week mode (default: week)</summary>
    public CalendarWeekMode WeekMode { get; set; }

    /// <summary>Work on monday</summary>
    public bool WorkMonday { get; set; }

    /// <summary>Work on tuesday</summary>
    public bool WorkTuesday { get; set; }

    /// <summary>Work on wednesday</summary>
    public bool WorkWednesday { get; set; }

    /// <summary>Work on thursday</summary>
    public bool WorkThursday { get; set; }

    /// <summary>Work on friday</summary>
    public bool WorkFriday { get; set; }

    /// <summary>Work on saturday</summary>
    public bool WorkSaturday { get; set; }

    /// <summary>Work on sunday</summary>
    public bool WorkSunday { get; set; }

    /// <summary>Custom attributes</summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}