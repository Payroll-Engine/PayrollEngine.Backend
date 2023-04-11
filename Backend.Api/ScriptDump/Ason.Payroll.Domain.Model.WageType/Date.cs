/* Date */
using System;

namespace Ason.Payroll.Client.Scripting;

/// <summary>Date specifications</summary>
public static class Date
{
    /// <summary>First month in year</summary>
    public static readonly int FirstMonthOfCalendarYear = 1;

    /// <summary>First day in month</summary>
    public static readonly int FirstDayOfMonth = 1;

    /// <summary>Number of months in a year</summary>
    public static readonly int MonthsInYear = 12;

    /// <summary>Last month in year</summary>
    public static readonly int LastMonthOfCalendarYear = MonthsInYear;

    /// <summary>Number of days in a week</summary>
    public static readonly int DaysInWeek = 7;

    /// <summary>Represents the smallest possible value of a time instant</summary>
    public static DateTime MinValue => DateTime.MinValue.ToUtc();

    /// <summary>Represents the largest possible value of a time instant</summary>
    public static DateTime MaxValue => DateTime.MaxValue.ToUtc();

    /// <summary>Gets a time instant that is set to the current date and time</summary>
    public static DateTime Now => DateTime.UtcNow;

    /// <summary>Gets a time instant that is set to the current day</summary>
    public static DateTime Today => Now.Date;

    /// <summary>Get the year start date in UTC</summary>
    public static DateTime YearStart(int year) =>
        new(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Get the year end date in UTC</summary>
    public static DateTime YearEnd(int year) =>
        YearStart(year).AddYears(1).AddTicks(-1);

    /// <summary>Get the month start date in UTC</summary>
    public static DateTime MonthStart(int year, int month) =>
        new(year, month, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Get the month end date in UTC</summary>
    public static DateTime MonthEnd(int year, int month) =>
        MonthStart(year, month).AddMonths(1).AddTicks(-1);

    /// <summary>Get the day start date and time in UTC</summary>
    public static DateTime DayStart(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Get the day end date and time in UTC</summary>
    public static DateTime DayEnd(int year, int month, int day) =>
        DayStart(year, month, day).AddDays(1).AddTicks(-1);

    /// <summary>Get the minimum date</summary>
    public static DateTime Min(DateTime left, DateTime right) =>
        left < right ? left : right;

    /// <summary>Get the maximum date</summary>
    public static DateTime Max(DateTime left, DateTime right) =>
        left > right ? left : right;

    /// <summary>Get the minimum timespan</summary>
    public static TimeSpan Min(TimeSpan left, TimeSpan right) =>
        left < right ? left : right;

    /// <summary>Get the maximum timespan</summary>
    public static TimeSpan Max(TimeSpan left, TimeSpan right) =>
        left > right ? left : right;
}