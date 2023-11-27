using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrunParameterService(IPayrunParameterRepository repository) :
    ChildApplicationService<IPayrunParameterRepository, PayrunParameter>(repository), IPayrunParameterService;