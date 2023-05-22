using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class RegulationShareService : RootApplicationService<IRegulationShareRepository, RegulationShare>,
    IRegulationShareService
{
    public RegulationShareService(IRegulationShareRepository repository) :
        base(repository)
    {
    }
}