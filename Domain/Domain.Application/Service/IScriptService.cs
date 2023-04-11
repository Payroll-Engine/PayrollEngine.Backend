using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IScriptService : IChildApplicationService<IScriptRepository, Script>
{
    /// <summary>
    /// Test if scripts exists
    /// </summary>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="scriptNames">The case names</param>
    /// <returns>True if any script exists</returns>
    Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<string> scriptNames);
}