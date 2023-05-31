using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll employee cases
/// </summary>
public abstract class EmployeeCaseValueController : CaseValueController<IEmployeeService,
    IEmployeeRepository, IEmployeeCaseValueRepository,
    DomainObject.Employee>
{
    protected EmployeeCaseValueController(IEmployeeService employeeService, ICaseValueService<IEmployeeCaseValueRepository, 
            DomainObject.CaseValue> caseValueService, IPayrollService payrollsService, IRegulationService regulationService,
        ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(employeeService, caseValueService, payrollsService, regulationService, lookupSetService, runtime)
    {
    }
}