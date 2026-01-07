using System.Threading.Tasks;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for case fields
/// </summary>
public interface ICaseFieldRepository : ITrackChildDomainRepository<CaseField, CaseFieldAudit>
{
    /// <summary>
    /// Determine if a case contains a case field name
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseId">The case id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>True if the case contains a case field with this name</returns>
    Task<bool> ExistsAnyAsync(IDbContext context, int caseId, IEnumerable<string> caseFieldNames);

    /// <summary>
    /// Get regulation case fields by name
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <param name="regulationId">The regulation id</param>
    /// <returns>The regulation case fields</returns>
    Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(IDbContext context, int tenantId, 
        IEnumerable<string> caseFieldNames, int? regulationId = null);
}