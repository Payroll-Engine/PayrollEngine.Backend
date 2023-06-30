using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeCaseValueService : CaseValueService<IEmployeeCaseValueRepository>, IEmployeeCaseValueService
{
    public EmployeeCaseValueService(IEmployeeCaseValueRepository employeeCaseValueRepository) :
        base(employeeCaseValueRepository)
    {
    }
}