﻿using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for tenant users
/// </summary>
public interface IUserRepository : IChildDomainRepository<User>
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
    /// Test the user password
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="password">The password to test</param>
    /// <returns>True for a valid password</returns>
    Task<bool> TestPasswordAsync(IDbContext context, int tenantId, int userId, string password);

    /// <summary>
    /// Change the user password
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="changeRequest">The password change request including the new and existing password</param>
    /// <returns>True if the user with this identifier exists</returns>
    System.Threading.Tasks.Task UpdatePasswordAsync(IDbContext context, int tenantId, int userId, PasswordChangeRequest changeRequest);
}