using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ILookupService : IChildApplicationService<ILookupRepository, Lookup>
{
    Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> lookupNames);
}