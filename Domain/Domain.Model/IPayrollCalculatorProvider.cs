using System.Globalization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provider for payroll calculators
/// </summary>
public interface IPayrollCalculatorProvider
{
    /// <summary>
    /// Create a payroll value calculator
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="culture">The culture to use</param>
    /// <param name="calendar">The payroll calendar</param>
    /// <returns>The payroll calculator</returns>
    IPayrollCalculator CreateCalculator(int tenantId, int? userId = null, 
        CultureInfo culture = null, Calendar calendar = null);
}