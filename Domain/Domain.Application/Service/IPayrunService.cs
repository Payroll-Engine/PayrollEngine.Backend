using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrunService : IChildApplicationService<IPayrunRepository, Payrun>
{
    Task RebuildAsync(int parentId, int itemId);
}