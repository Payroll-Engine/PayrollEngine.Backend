using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ScriptService
    (IScriptRepository repository) : ChildApplicationService<IScriptRepository, Script>(repository), IScriptService
{
    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> scriptNames) =>
        await Repository.ExistsAnyAsync(context, regulationId, scriptNames);
}