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
    public static Calendar DefaultCalendar => new();

    /// <inheritdoc />
    public IPayrollCalculator CreateCalculator(int tenantId, int? userId = null,
        CultureInfo culture = null, Calendar calendar = null) =>
        new PayrollCalculator(culture, calendar);
}