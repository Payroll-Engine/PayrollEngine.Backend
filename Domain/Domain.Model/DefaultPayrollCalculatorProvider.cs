using System;
using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Default provider for payroll calculators
/// </summary>
public class DefaultPayrollCalculatorProvider : IPayrollCalculatorProvider
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

    /// <inheritdoc />
    public IPayrollCalculator CreateCalculator(CalendarCalculationMode calculationMode,
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