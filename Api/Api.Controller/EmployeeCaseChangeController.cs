using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll employee case changes
/// </summary>
public abstract class EmployeeCaseChangeController : CaseChangeController<IEmployeeService,
    IEmployeeRepository, IEmployeeCaseChangeRepository,
    DomainObject.Employee, DomainObject.CaseChange, ApiObject.CaseChange>
{
    protected EmployeeCaseChangeController(IEmployeeService employeeService, IEmployeeCaseChangeService caseChangeService,
        ICaseFieldService caseFieldService, IDivisionService divisionService, IUserService userService, IControllerRuntime runtime) :
        base(employeeService, caseChangeService, caseFieldService, divisionService, userService, runtime)
    {
    }
}