using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Result of a <see cref="PayrunProcessor"/> preview execution.
/// Contains the in-memory payrun job and the calculated result set for the single employee.
/// </summary>
public class PayrunProcessResult
{
    /// <summary>The payrun job (always populated, not persisted in preview mode)</summary>
    public PayrunJob PayrunJob { get; init; }

    /// <summary>
    /// The calculated result set for the single preview employee.
    /// Null when the employee produced no results.
    /// </summary>
    public PayrollResultSet ResultSet { get; init; }
}
