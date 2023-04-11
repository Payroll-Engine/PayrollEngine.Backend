using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class RegulationPermissionService : RootApplicationService<IRegulationPermissionRepository, RegulationPermission>,
    IRegulationPermissionService
{
    public RegulationPermissionService(IRegulationPermissionRepository repository) :
        base(repository)
    {
    }
}