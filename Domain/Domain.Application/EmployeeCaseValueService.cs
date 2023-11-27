using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeCaseValueService(IEmployeeCaseValueRepository employeeCaseValueRepository) :
    CaseValueService<IEmployeeCaseValueRepository>(employeeCaseValueRepository), IEmployeeCaseValueService;