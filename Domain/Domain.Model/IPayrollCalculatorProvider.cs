using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provider for payroll calculators
/// </summary>
public interface IPayrollCalculatorProvider
{
    /// <summary>
    /// Create a case value calculator bases on the calculation mode
    /// </summary>
    /// <param name="calculationMode">The calendar calculation mode</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="culture">The culture to use</param>
    /// <param name="calendar">The calendar configuration</param>
    /// <returns>The case value calculator</returns>
    public IPayrollCalculator CreateCalculator(CalendarCalculationMode calculationMode,
        int tenantId, int? userId = null, CalendarConfiguration calendar = null, CultureInfo culture = null);
}