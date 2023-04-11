using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseFieldService : ChildApplicationService<ICaseFieldRepository, CaseField>, ICaseFieldService
{
    public CaseFieldService(ICaseFieldRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAnyAsync(int caseId, IEnumerable<string> caseFieldNames) =>
        await Repository.ExistsAnyAsync(caseId, caseFieldNames);

    public virtual async Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(int tenantId, IEnumerable<string> caseFieldNames) =>
        await Repository.GetRegulationCaseFieldsAsync(tenantId, caseFieldNames);
}