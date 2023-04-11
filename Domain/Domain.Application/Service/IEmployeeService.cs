using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IEmployeeService : IChildApplicationService<IEmployeeRepository, Employee>
{
    /// <summary>
    /// Determine if the employee existing by the identifier
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="identifier">The user identifier</param>
    /// <returns>True if the employee with this identifier exists</returns>
    Task<bool> ExistsAnyAsync(int tenantId, string identifier);
}