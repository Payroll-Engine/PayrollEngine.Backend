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

    public async Task<bool> ExistsAnyAsync(IDbContext context, int tenantId, string identifier) =>
        await Repository.ExistsAnyAsync(context, tenantId, identifier);

    public async Task UpdatePasswordAsync(IDbContext context, int tenantId, int userId, string password) =>
        await Repository.UpdatePasswordAsync(context, tenantId, userId, password);
}