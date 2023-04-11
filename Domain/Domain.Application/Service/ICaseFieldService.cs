using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ICaseFieldService : IChildApplicationService<ICaseFieldRepository, CaseField>
{
    /// <summary>
    /// Determine if a case contains a case field name
    /// </summary>
    /// <param name="caseId">The case id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>True if the case contains a case field with this name</returns>
    Task<bool> ExistsAnyAsync(int caseId, IEnumerable<string> caseFieldNames);

    /// <summary>
    /// Get regulation case fields
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The regulation case fields</returns>
    Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(int tenantId, IEnumerable<string> caseFieldNames);
}