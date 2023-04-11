using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeService : ChildApplicationService<IEmployeeRepository, Employee>, IEmployeeService
{
    public EmployeeService(IEmployeeRepository repository) :
        base(repository)
    {
    }
    public virtual async Task<bool> ExistsAnyAsync(int tenantId, string identifier) =>
        await Repository.ExistsAnyAsync(tenantId, identifier);
}