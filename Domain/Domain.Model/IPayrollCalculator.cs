﻿using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a calculator for case values
/// </summary>
public interface IPayrollCalculator
{
    /// <summary>
    /// Get payrun period from a specific moment
    /// </summary>
    /// <param name="periodMoment">The moment within the payrun period</param>
    /// <returns>The payroll period</returns>
    IPayrollPeriod GetPayrunPeriod(DateTime periodMoment);

    /// <summary>
    /// Get payrun cycle from a specific moment
    /// </summary>
    /// <param name="cycleMoment">The moment within the payrun cycle</param>
    /// <returns>The payroll cycle</returns>
    IPayrollPeriod GetPayrunCycle(DateTime cycleMoment);

    /// <summary>
    /// Calculate the case period value over a time period
    /// </summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case period value</returns>
    decimal? CalculateCasePeriodValue(CaseValueCalculation calculation);
}