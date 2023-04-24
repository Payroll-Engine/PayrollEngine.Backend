using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application.Service;

public interface IUserService : IChildApplicationService<IUserRepository, User>
{
    /// <summary>
    /// Determine if the user existing by the identifier
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="identifier">The user identifier</param>
    /// <returns>True if the user with this identifier exists</returns>
    Task<bool> ExistsAnyAsync(IDbContext context, int tenantId, string identifier);

    /// <summary>
    /// Update the user password
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="password">The new user password</param>
    /// <returns>True if the user with this identifier exists</returns>
    Task UpdatePasswordAsync(IDbContext context, int tenantId, int userId, string password);
}