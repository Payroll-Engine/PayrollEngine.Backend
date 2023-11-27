using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LogService
    (ILogRepository repository) : ChildApplicationService<ILogRepository, Domain.Model.Log>(repository), ILogService;