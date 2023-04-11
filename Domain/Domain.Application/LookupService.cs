using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupService : ChildApplicationService<ILookupRepository, Lookup>, ILookupService
{
    public LookupService(ILookupRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<string> lookupNames) =>
        await Repository.ExistsAnyAsync(regulationId, lookupNames);
}