using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll collectors
/// </summary>
public interface ICollectorRepository : IScriptTrackDomainObjectRepository<Collector, CollectorAudit>
{
    /// <summary>
    /// Determine if any of the collector names are existing
    /// </summary>
    /// <param name="regulationId">The payroll regulation id</param>
    /// <param name="collectorNames">The collector names</param>
    /// <returns>True if any collector with a name exists</returns>
    Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<string> collectorNames);
}