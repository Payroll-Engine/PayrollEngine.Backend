using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ScriptService : ChildApplicationService<IScriptRepository, Script>, IScriptService
{
    public ScriptService(IScriptRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> scriptNames) =>
        await Repository.ExistsAnyAsync(context, regulationId, scriptNames);
}