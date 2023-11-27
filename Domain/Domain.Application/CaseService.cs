using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseService
    (ICaseRepository repository) : ScriptTrackChildApplicationService<ICaseRepository, Case, CaseAudit>(repository),
        ICaseService
{
    public async Task<Case> GetAsync(IDbContext context, int tenantId, int regulationId, string name) =>
        (await Repository.QueryAsync(context, tenantId, name, regulationId)).FirstOrDefault();
}