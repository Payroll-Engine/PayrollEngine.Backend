using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WageTypeResultService : ChildApplicationService<IWageTypeResultRepository, WageTypeResult>, IWageTypeResultService
{
    public WageTypeResultService(IWageTypeResultRepository repository) :
        base(repository)
    {
    }
}