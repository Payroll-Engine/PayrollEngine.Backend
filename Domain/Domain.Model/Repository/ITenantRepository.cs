using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for tenants
/// </summary>
public interface ITenantRepository : IRootDomainRepository<Tenant>
{
    /// <summary>
    /// Test if tenant exists
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="identifier">The tenant identifier</param>
    /// <returns>True any collector exists</returns>
    Task<bool> ExistsAsync(IDbContext context, string identifier);

    /// <summary>
    /// Update statistics for all database tables using FULLSCAN.
    /// Call after large bulk imports (lookup values) or after accumulating
    /// many payrun results to prevent query plan degradation.
    /// </summary>
    /// <param name="context">The database context</param>
    System.Threading.Tasks.Task UpdateStatisticsAsync(IDbContext context);
}