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
    /// The time map of calendar period case values
    /// </summary>
    CalendarTimeMap TimeMap { get; }

    /// <summary>
    /// The assignment of the period to the cycle and to the regulation date
    /// </summary>
    CalendarCycleAssignment CycleAssignment { get; }

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
    /// Get the calendar pay date of the payrun period including a moment
    /// </summary>
    /// <param name="periodMoment">The moment within the payrun period</param>
    /// <returns>The pay date: period end date plus the calendar pay date offset</returns>
    DateTime GetPayDate(DateTime periodMoment);

    /// <summary>
    /// Get the calendar cycle date of the payrun period including a moment
    /// </summary>
    /// <param name="periodMoment">The moment within the payrun period</param>
    /// <returns>The calendar pay date or the period end, depending on the cycle assignment</returns>
    DateTime GetCycleDate(DateTime periodMoment);

    /// <summary>
    /// Get the start of the first payrun period of a cycle, based on the calendar cycle dates
    /// </summary>
    /// <param name="cycleStart">The cycle start</param>
    /// <returns>The start of the first period assigned to the cycle</returns>
    DateTime GetCyclePeriodStart(DateTime cycleStart);

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