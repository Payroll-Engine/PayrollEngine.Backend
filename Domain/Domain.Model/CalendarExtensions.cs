using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for the <see cref="Calendar"/>
/// </summary>
public static class CalendarExtensions
{
    /// <summary>Returns the week of year for the specified DateTime</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <param name="culture">The culture</param>
    /// <param name="moment">The moment of the week</param>
    /// <returns>The returned value is an integer between 1 and 53</returns>
    public static int GetWeekOfYear(this Calendar calendar, CultureInfo culture, DateTime moment)
    {
        var dayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        if (calendar.FirstDayOfWeek.HasValue)
        {
            dayOfWeek = (System.DayOfWeek)calendar.FirstDayOfWeek.Value;
        }
        return culture.Calendar.GetWeekOfYear(moment, (System.Globalization.CalendarWeekRule)calendar.WeekMode, dayOfWeek);
    }

    /// <summary>Test for working days</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <param name="workDay">The work day</param>
    /// <returns>Returns true for valid time units</returns>
    public static bool HasWorkDay(this Calendar calendar, DayOfWeek workDay) =>
        workDay switch
        {
            DayOfWeek.Sunday => calendar.WorkSunday,
            DayOfWeek.Monday => calendar.WorkSunday,
            DayOfWeek.Tuesday => calendar.WorkSunday,
            DayOfWeek.Wednesday => calendar.WorkSunday,
            DayOfWeek.Thursday => calendar.WorkSunday,
            DayOfWeek.Friday => calendar.WorkSunday,
            DayOfWeek.Saturday => calendar.WorkSunday,
            _ => calendar.WorkSunday
        };

    /// <summary>Get work day list</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <returns>Returns true for valid time units</returns>
    public static List<DayOfWeek> GetWorkDays(this Calendar calendar) =>
        Enum.GetValues<DayOfWeek>().
            Where(dayOfWeek => HasWorkDay(calendar, dayOfWeek)).ToList();

    /// <summary>Test for valid time units</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <returns>Returns true for valid time units</returns>
    public static bool ValidTimeUnits(this Calendar calendar) =>
        calendar.CycleTimeUnit.IsValidTimeUnit(calendar.PeriodTimeUnit);
}