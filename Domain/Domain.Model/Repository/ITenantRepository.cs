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
    /// <param name="identifier">The tenant identifier</param>
    /// <returns>True any collector exists</returns>
    Task<bool> ExistsAsync(string identifier);
}