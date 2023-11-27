using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupSetService
    (ILookupSetRepository repository) : ChildApplicationService<ILookupSetRepository, LookupSet>(repository),
        ILookupSetService
{
    public async Task<LookupSet> GetSetAsync(IDbContext context, int tenantId, int regulationId, int lookupId) =>
        await Repository.GetLookupSetAsync(context, tenantId, regulationId, lookupId);
}