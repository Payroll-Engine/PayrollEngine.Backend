using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll scripts
/// </summary>
public interface IScriptRepository : ITrackChildDomainRepository<Script, ScriptAudit>
{
    /// <summary>
    /// Determine if any of the script names are existing
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="regulationId">The payroll regulation id</param>
    /// <param name="scriptNames">The script names</param>
    /// <returns>True if any script with this name exists</returns>
    Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> scriptNames);

    /// <summary>
    /// Get script from specific function types
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="regulationId">The payroll regulation id</param>
    /// <param name="functionTypes">The function types</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>The function types scripts</returns>
    Task<IEnumerable<Script>> GetFunctionScriptsAsync(IDbContext context, int regulationId, 
        List<FunctionType> functionTypes = null, DateTime? evaluationDate = null);
}