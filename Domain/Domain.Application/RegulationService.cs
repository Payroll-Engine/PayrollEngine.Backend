using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class RegulationService
    (IRegulationRepository repository) : ChildApplicationService<IRegulationRepository, Regulation>(repository),
        IRegulationService;