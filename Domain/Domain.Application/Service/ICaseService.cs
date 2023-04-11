using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ICaseService : IScriptTrackChildApplicationService<ICaseRepository, Case, CaseAudit>
{
    /// <summary>
    /// Get case by name
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="name">The case name</param>
    /// <returns>True if the case exists</returns>
    Task<Case> GetAsync(int tenantId, int regulationId, string name);

    /// <summary>
    /// Test if case exists
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="name">The case name</param>
    /// <returns>True if the case exists</returns>
    Task<bool> ExistsAsync(int tenantId, int regulationId, string name);
}