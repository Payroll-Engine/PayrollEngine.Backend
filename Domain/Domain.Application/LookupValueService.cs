using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupValueService : ChildApplicationService<ILookupValueRepository, LookupValue>, ILookupValueService
{
    public LookupValueService(ILookupValueRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAsync(IDbContext context, int lookupId, string key, decimal? rangeValue = null) =>
        await Repository.ExistsAsync(context, lookupId, key, rangeValue);
}