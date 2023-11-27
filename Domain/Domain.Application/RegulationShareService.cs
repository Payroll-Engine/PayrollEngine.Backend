using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class RegulationShareService(IRegulationShareRepository repository) :
    RootApplicationService<IRegulationShareRepository, RegulationShare>(repository),
    IRegulationShareService;