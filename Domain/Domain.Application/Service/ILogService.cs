using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ILogService : IChildApplicationService<ILogRepository, Domain.Model.Log>
{
}