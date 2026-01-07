using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ITenantService : IRootApplicationService<ITenantRepository, Tenant>
{
    /// <summary>
    /// Determine if the tenant existing by the identifier
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="identifier">The tenant identifier</param>
    /// <returns>True if the tenant with this identifier exists</returns>
    Task<bool> ExistsAsync(IDbContext context, string identifier);

    /// <summary>
    /// Get system script actions
    /// </summary>
    /// <param name="functionType">The function types (default: all)</param>
    /// <returns>The system action infos</returns>
    Task<IEnumerable<ActionInfo>> GetSystemScriptActionsAsync(FunctionType functionType = FunctionType.All);

    /// <summary>
    /// Get system script action properties
    /// </summary>
    /// <param name="functionType">The function types (default: all)</param>
    /// <param name="readOnly">Read only properties (default: true)</param>
    /// <returns>The system action property infos</returns>
    Task<IEnumerable<ActionInfo>> GetSystemScriptActionPropertiesAsync(FunctionType functionType = FunctionType.All,
        bool readOnly = true);
}