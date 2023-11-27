using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupService
    (ILookupRepository repository) : ChildApplicationService<ILookupRepository, Lookup>(repository), ILookupService
{
    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> lookupNames) =>
        await Repository.ExistsAnyAsync(context, regulationId, lookupNames);
}