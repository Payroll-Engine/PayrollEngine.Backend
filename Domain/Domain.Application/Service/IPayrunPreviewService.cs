using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application.Service;

/// <summary>
/// Service for previewing payrun job results without persisting to the database.
/// </summary>
public interface IPayrunPreviewService
{
    /// <summary>
    /// Executes a payrun preview for a single employee and returns the results as a <see cref="PayrollResultSet"/>.
    /// No data is persisted to the database.
    /// </summary>
    /// <param name="tenant">The owning tenant</param>
    /// <param name="payrun">The payrun definition</param>
    /// <param name="jobInvocation">The job invocation with exactly one employee identifier</param>
    /// <returns>The payroll result set containing wage type results, collector results, and payrun results</returns>
    Task<PayrollResultSet> PreviewAsync(Tenant tenant, Payrun payrun, PayrunJobInvocation jobInvocation);
}
