using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class CaseValueService<TRepo> :
    ChildApplicationService<TRepo, CaseValue>, ICaseValueService<TRepo, CaseValue>
    where TRepo : class, ICaseValueRepository
{
    protected CaseValueService(TRepo caseValueRepository) :
        base(caseValueRepository)
    {
    }

    public Task<IEnumerable<string>> GetCaseValueSlotsAsync(IDbContext context, int parentId, string caseFieldName) =>
        Repository.GetCaseValueSlotsAsync(context, parentId, caseFieldName);
}