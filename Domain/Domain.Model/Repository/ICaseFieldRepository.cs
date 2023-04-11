using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for case fields
/// </summary>
public interface ICaseFieldRepository : ITrackChildDomainRepository<CaseField, CaseFieldAudit>
{
    /// <summary>
    /// Determine if a case contains a case field name
    /// </summary>
    /// <param name="caseId">The case id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>True if the case contains a case field with this name</returns>
    Task<bool> ExistsAnyAsync(int caseId, IEnumerable<string> caseFieldNames);

    /// <summary>
    /// Get regulation case fields by name
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <param name="regulationId">The regulation id</param>
    /// <returns>The regulation case fields</returns>
    Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(int tenantId, IEnumerable<string> caseFieldNames,
        int? regulationId = null);
}