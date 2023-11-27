using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll employee case changes
/// </summary>
public abstract class EmployeeCaseChangeController(IEmployeeService employeeService,
        IEmployeeCaseChangeService caseChangeService,
        IControllerRuntime runtime)
    : CaseChangeController<IEmployeeService,
    IEmployeeRepository, IEmployeeCaseChangeRepository,
    DomainObject.Employee, DomainObject.CaseChange, ApiObject.CaseChange>(employeeService, caseChangeService, runtime);