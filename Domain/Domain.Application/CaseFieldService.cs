using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Domain.Application;

public class CaseFieldService(ICaseFieldRepository repository) : 
    ChildApplicationService<ICaseFieldRepository, CaseField>(repository), ICaseFieldService
{
    public async Task<bool> ExistsAnyAsync(IDbContext context, int caseId, IEnumerable<string> caseFieldNames) =>
        await Repository.ExistsAnyAsync(context, caseId, caseFieldNames);

    public async Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(IDbContext context, int tenantId, IEnumerable<string> caseFieldNames) =>
        await Repository.GetRegulationCaseFieldsAsync(context, tenantId, caseFieldNames);
}