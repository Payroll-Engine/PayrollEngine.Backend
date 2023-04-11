using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseService : ScriptTrackChildApplicationService<ICaseRepository, Case, CaseAudit>, ICaseService
{
    public CaseService(ICaseRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<Case> GetAsync(int tenantId, int regulationId, string name) =>
        (await Repository.QueryAsync(tenantId, name, regulationId)).FirstOrDefault();

    public virtual async Task<bool> ExistsAsync(int tenantId, int regulationId, string name) =>
        await Repository.ExistsAsync(tenantId, regulationId, name);
}