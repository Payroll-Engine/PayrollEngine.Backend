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

    public virtual async Task<bool> ExistsAnyAsync(IDbContext context, int caseId, IEnumerable<string> caseFieldNames) =>
        await Repository.ExistsAnyAsync(context, caseId, caseFieldNames);

    public virtual async Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(IDbContext context, int tenantId, IEnumerable<string> caseFieldNames) =>
        await Repository.GetRegulationCaseFieldsAsync(context, tenantId, caseFieldNames);
}