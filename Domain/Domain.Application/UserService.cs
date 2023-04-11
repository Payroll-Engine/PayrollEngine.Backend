using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public class UserService : ChildApplicationService<IUserRepository, User>, IUserService
{
    public UserService(IUserRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAnyAsync(int tenantId, string identifier) =>
        await Repository.ExistsAnyAsync(tenantId, identifier);

    public virtual async Task UpdatePasswordAsync(int tenantId, int userId, string password) =>
        await Repository.UpdatePasswordAsync(tenantId, userId, password);
}