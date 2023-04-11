using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ILookupValueService : IChildApplicationService<ILookupValueRepository, LookupValue>
{
    Task<bool> ExistsAsync(int lookupId, string key, decimal? rangeValue = null);
}