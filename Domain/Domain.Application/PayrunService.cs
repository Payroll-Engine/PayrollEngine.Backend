using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public class PayrunService : ChildApplicationService<IPayrunRepository, Payrun>, IPayrunService
{
    public PayrunService(IPayrunRepository repository) :
        base(repository)
    {
    }

    public virtual async Task RebuildAsync(int parentId, int itemId) =>
        await Repository.RebuildAsync(parentId, itemId);
}