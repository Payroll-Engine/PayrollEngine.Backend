using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ICollectorService : IScriptTrackChildApplicationService<ICollectorRepository, Collector, CollectorAudit>
{
    /// <summary>
    /// Test if collectors exists
    /// </summary>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="collectorNames">The collector names</param>
    /// <returns>True any collector exists</returns>
    Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<string> collectorNames);
}