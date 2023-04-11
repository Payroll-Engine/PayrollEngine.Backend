using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class DivisionService : ChildApplicationService<IDivisionRepository, Division>, IDivisionService
{
    public DivisionService(IDivisionRepository repository) :
        base(repository)
    {
    }
}