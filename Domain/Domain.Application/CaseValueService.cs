using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class CaseValueService<TRepo>(TRepo caseValueRepository) :
    ChildApplicationService<TRepo, CaseValue>(caseValueRepository), ICaseValueService<TRepo, CaseValue>
    where TRepo : class, ICaseValueRepository
{
    public Task<IEnumerable<string>> GetCaseValueSlotsAsync(IDbContext context, int parentId, string caseFieldName) =>
        Repository.GetCaseValueSlotsAsync(context, parentId, caseFieldName);
}