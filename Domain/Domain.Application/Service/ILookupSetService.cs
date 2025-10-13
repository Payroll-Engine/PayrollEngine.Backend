using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ILookupSetService : IChildApplicationService<ILookupSetRepository, LookupSet>
{
    /// <summary>
    /// Get a lookup set, including the lookup values
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <returns>The lookup set</returns>
    Task<LookupSet> GetSetAsync(IDbContext context, int regulationId, int lookupId);
}