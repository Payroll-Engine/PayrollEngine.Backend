using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll layers
/// </summary>
public interface IPayrollLayerRepository : IChildDomainRepository<PayrollLayer>
{
    /// <summary>
    /// Determine if a payroll layer with the same level and priority exists
    /// </summary>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="level">The layer level</param>
    /// <param name="priority">The layer priority</param>
    /// <returns>True if the lookup row with any of the key exists</returns>
    Task<bool> ExistsAsync(int payrollId, int level, int priority);
}