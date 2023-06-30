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

    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> collectorNames) =>
        await Repository.ExistsAnyAsync(context, regulationId, collectorNames);
}