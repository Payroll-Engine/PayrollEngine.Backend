using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class DivisionService
    (IDivisionRepository repository) : ChildApplicationService<IDivisionRepository, Division>(repository),
        IDivisionService;