using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollLayerService
    (IPayrollLayerRepository repository) : ChildApplicationService<IPayrollLayerRepository, PayrollLayer>(repository),
        IPayrollLayerService
{
    public async Task<bool> ExistsAsync(IDbContext context, int payrollId, int level, int priority) =>
        await Repository.ExistsAsync(context, payrollId, level, priority);
}