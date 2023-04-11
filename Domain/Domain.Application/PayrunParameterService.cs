using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrunParameterService : ChildApplicationService<IPayrunParameterRepository, PayrunParameter>, IPayrunParameterService
{
    public PayrunParameterService(IPayrunParameterRepository repository) :
        base(repository)
    {
    }
}