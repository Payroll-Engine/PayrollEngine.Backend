using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CompanyCaseValueService : CaseValueService<ICompanyCaseValueRepository>, ICompanyCaseValueService
{
    public CompanyCaseValueService(ICompanyCaseValueRepository companyCaseValueRepository) :
        base(companyCaseValueRepository)
    {
    }
}