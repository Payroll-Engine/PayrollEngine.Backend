using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for the <see cref="Calendar"/>
/// </summary>
/// <remarks>Client service contains a copy</remarks>
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
    private static bool IsWorkDay(this Calendar calendar, DayOfWeek workDay) =>
        workDay switch
        {
            DayOfWeek.Sunday => calendar.WorkSunday,
            DayOfWeek.Monday => calendar.WorkMonday,
            DayOfWeek.Tuesday => calendar.WorkThursday,
            DayOfWeek.Wednesday => calendar.WorkWednesday,
            DayOfWeek.Thursday => calendar.WorkThursday,
            DayOfWeek.Friday => calendar.WorkFriday,
            DayOfWeek.Saturday => calendar.WorkSaturday,
            _ => calendar.WorkSunday
        };

    /// <summary>Test for working day</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <param name="moment">Test day</param>
    /// <returns>Returns true for valid time units</returns>
    public static bool IsWorkDay(this Calendar calendar, DateTime moment) =>
        // week mode
        calendar.WeekMode == CalendarWeekMode.Week ||
        // work week
        IsWorkDay(calendar, (DayOfWeek)moment.DayOfWeek);

    /// <summary>Get week days</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <returns>Returns true for valid time units</returns>
    public static List<DayOfWeek> GetWeekDays(this Calendar calendar) =>
        Enum.GetValues<DayOfWeek>().
            Where(dayOfWeek => IsWorkDay(calendar, dayOfWeek)).ToList();

    /// <summary>Get previous working days</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <param name="moment">The start moment (not included in results)</param>
    /// <param name="count">The number of days (default: 1)</param>
    /// <returns>Returns true for valid time units</returns>
    public static List<DateTime> GetPreviousWorkDays(this Calendar calendar, DateTime moment, int count = 1)
    {
        var days = new List<DateTime>();
        for (var i = 0; i < count; i++)
        {
            var day = moment.AddDays(-i).Date;
            if (IsWorkDay(calendar, day))
            {
                days.Add(day);
            }
        }
        return days;
    }

    /// <summary>Get next working days</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <param name="moment">The start moment (not included in results)</param>
    /// <param name="count">The number of days (default: 1)</param>
    /// <returns>Returns true for valid time units</returns>
    public static List<DateTime> GetNextWorkDays(this Calendar calendar, DateTime moment, int count = 1)
    {
        var days = new List<DateTime>();
        for (var i = 0; i < count; i++)
        {
            var day = moment.AddDays(i).Date;
            if (IsWorkDay(calendar, day))
            {
                days.Add(day);
            }
        }
        return days;
    }

    /// <summary>Test for valid time units</summary>
    /// <param name="calendar">The payroll calendar</param>
    /// <returns>Returns true for valid time units</returns>
    public static bool ValidTimeUnits(this Calendar calendar) =>
        calendar.CycleTimeUnit.IsValidTimeUnit(calendar.PeriodTimeUnit);
}