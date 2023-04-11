using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LogService : ChildApplicationService<ILogRepository, Domain.Model.Log>, ILogService
{
    public LogService(ILogRepository repository) :
        base(repository)
    {
    }
}