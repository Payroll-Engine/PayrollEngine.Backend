using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for cases
/// </summary>
public interface ICaseRepository : IScriptTrackDomainObjectRepository<Case, CaseAudit>
{
    /// <summary>
    /// Query cases by name
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseName">The case name</param>
    /// <param name="regulationId">The regulation id</param>
    /// <returns>The regulation cases</returns>
    Task<IEnumerable<Case>> QueryAsync(IDbContext context, int tenantId, string caseName, int? regulationId = null);
}