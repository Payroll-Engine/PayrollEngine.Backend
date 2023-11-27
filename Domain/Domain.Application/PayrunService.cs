using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public class PayrunService
    (IPayrunRepository repository) : ChildApplicationService<IPayrunRepository, Payrun>(repository), IPayrunService
{
    public async Task RebuildAsync(IDbContext context, int parentId, int itemId) =>
        await Repository.RebuildAsync(context, parentId, itemId);
}