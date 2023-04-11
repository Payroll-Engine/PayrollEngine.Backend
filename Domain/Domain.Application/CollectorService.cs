using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CollectorService : ScriptTrackChildApplicationService<ICollectorRepository, Collector, CollectorAudit>, ICollectorService
{
    public CollectorService(ICollectorRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<string> collectorNames) =>
        await Repository.ExistsAnyAsync(regulationId, collectorNames);
}