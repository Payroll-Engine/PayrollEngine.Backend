using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CollectorService(ICollectorRepository repository) :
    ScriptTrackChildApplicationService<ICollectorRepository, Collector, CollectorAudit>(repository), ICollectorService
{
    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> collectorNames) =>
        await Repository.ExistsAnyAsync(context, regulationId, collectorNames);
}