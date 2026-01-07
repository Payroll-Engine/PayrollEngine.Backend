using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ICaseFieldService : IChildApplicationService<ICaseFieldRepository, CaseField>
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
    /// Get regulation case fields
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The regulation case fields</returns>
    Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(IDbContext context, 
        int tenantId, IEnumerable<string> caseFieldNames);
}