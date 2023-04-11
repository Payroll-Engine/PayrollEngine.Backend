using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollLayerService : IChildApplicationService<IPayrollLayerRepository, PayrollLayer>
{
    /// <summary>
    /// Determine if a payroll layer with the same level and priority exists
    /// </summary>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="level">The layer level</param>
    /// <param name="priority">The layer priority</param>
    /// <returns>True if the payroll layer exists</returns>
    Task<bool> ExistsAsync(int payrollId, int level, int priority);
}