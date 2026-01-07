using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a calculator for case values
/// </summary>
public interface IPayrollCalculator
{
    /// <summary>
    /// The cycle time unit
    /// </summary>
    CalendarTimeUnit CycleTimeUnit { get; }

    /// <summary>
    /// The period time unit
    /// </summary>
    CalendarTimeUnit PeriodTimeUnit { get; }

    /// <summary>
    /// Get payrun cycle from a specific moment
    /// </summary>
    /// <param name="cycleMoment">The moment within the payrun cycle</param>
    /// <returns>The payroll cycle</returns>
    IPayrollPeriod GetPayrunCycle(DateTime cycleMoment);

    /// <summary>
    /// Get payrun period from a specific moment
    /// </summary>
    /// <param name="periodMoment">The moment within the payrun period</param>
    /// <returns>The payroll period</returns>
    IPayrollPeriod GetPayrunPeriod(DateTime periodMoment);

    /// <summary>
    /// Count the calendar days from a date period
    /// </summary>
    /// <param name="period">The date period</param>
    int GetCalendarDayCount(DatePeriod period);

    /// <summary>
    /// Calculate the case period value over a time period
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case period value</returns>
    decimal? CalculateCasePeriodValue(CaseValueCalculation calculation);
}