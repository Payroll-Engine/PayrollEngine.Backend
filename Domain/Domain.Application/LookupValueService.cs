using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupValueService
    (ILookupValueRepository repository) : ChildApplicationService<ILookupValueRepository, LookupValue>(repository),
        ILookupValueService
{
    public async Task<bool> ExistsAsync(IDbContext context, int lookupId, string key, decimal? rangeValue = null) =>
        await Repository.ExistsAsync(context, lookupId, key, rangeValue);
}