using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollLayerService : ChildApplicationService<IPayrollLayerRepository, PayrollLayer>, IPayrollLayerService
{
    public PayrollLayerService(IPayrollLayerRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAsync(int payrollId, int level, int priority) =>
        await Repository.ExistsAsync(payrollId, level, priority);
}