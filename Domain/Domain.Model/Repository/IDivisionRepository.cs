using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for divisions
/// </summary>
public interface IDivisionRepository : IChildDomainRepository<Division>
{
    /// <summary>
    /// Get multiple divisions by ids
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="divisionIds">The division ids</param>
    /// <returns>The divisions matching the ids</returns>
    Task<IEnumerable<Division>> GetByIdsAsync(IDbContext context, int tenantId, IEnumerable<int> divisionIds);

    /// <summary>
    /// Get division by name
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="name">The division name</param>
    /// <returns>The division matching the name</returns>
    Task<Division> GetByNameAsync(IDbContext context, int tenantId, string name);
}