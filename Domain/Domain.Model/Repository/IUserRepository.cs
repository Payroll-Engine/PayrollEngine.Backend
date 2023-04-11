using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for tenant users
/// </summary>
public interface IUserRepository : IChildDomainRepository<User>
{
    /// <summary>
    /// Determine if the user existing by the identifier
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="identifier">The user identifier</param>
    /// <returns>True if the user with this identifier exists</returns>
    Task<bool> ExistsAnyAsync(int tenantId, string identifier);

    /// <summary>
    /// Change the user password
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="password">The new user password, use null to reset the password</param>
    /// <returns>True if the user with this identifier exists</returns>
    System.Threading.Tasks.Task UpdatePasswordAsync(int tenantId, int userId, string password);
}