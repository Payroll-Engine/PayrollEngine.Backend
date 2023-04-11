using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Factory for payroll calculators
/// </summary>
public static class PayrollCalculatorFactory
{
    /// <summary>
    /// Default calendar
    /// </summary>
    public static CalendarConfiguration DefaultCalendar => new()
    {
        FirstMonthOfYear = Month.January, // calendar year
        AverageMonthDays = 30M, // switzerland
        AverageWorkDays = 21.75M, // switzerland
        WorkingDays = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
        }
    };

    /// <summary>
    /// Create a case value calculator bases on the calculation mode
    /// </summary>
    /// <param name="calculationMode">The calendar calculation mode</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="culture">The culture to use</param>
    /// <param name="calendar">The calendar configuration</param>
    /// <returns>The case value calculator</returns>
    public static IPayrollCalculator CreateCalculator(CalendarCalculationMode calculationMode,
        int tenantId, int? userId = null, CalendarConfiguration calendar = null, CultureInfo culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        calendar ??= DefaultCalendar;

        var payrollCalendar = new PayrollCalendar(calendar, tenantId, userId, culture);
        return calculationMode switch
        {
            CalendarCalculationMode.MonthCalendarDay => new MonthCalendarDayPayrollCalculator(payrollCalendar),
            CalendarCalculationMode.MonthAverageDay => new MonthAverageDayPayrollCalculator(payrollCalendar),
            CalendarCalculationMode.MonthEffectiveWorkDay => new MonthEffectiveWorkDayPayrollCalculator(payrollCalendar),
            CalendarCalculationMode.MonthAverageWorkDay => new MonthAverageWorkDayPayrollCalculator(payrollCalendar),
            CalendarCalculationMode.WeekCalendarDay => new WeekDayPayrollCalculator(payrollCalendar),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}