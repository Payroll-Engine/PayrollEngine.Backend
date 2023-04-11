using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CollectorResultService : ChildApplicationService<ICollectorResultRepository, CollectorResult>, ICollectorResultService
{
    public CollectorResultService(ICollectorResultRepository repository) :
        base(repository)
    {
    }
}