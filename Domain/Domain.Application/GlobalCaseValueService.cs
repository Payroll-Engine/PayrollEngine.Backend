using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class GlobalCaseValueService : CaseValueService<IGlobalCaseValueRepository>, IGlobalCaseValueService
{
    public GlobalCaseValueService(IGlobalCaseValueRepository globalCaseValueRepository) :
        base(globalCaseValueRepository)
    {
    }
}