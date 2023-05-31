using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the employees
/// </summary>
public abstract class EmployeeController : RepositoryChildObjectController<ITenantService, IEmployeeService,
    ITenantRepository, IEmployeeRepository,
    DomainObject.Tenant, DomainObject.Employee, ApiObject.Employee>
{
    protected EmployeeController(ITenantService tenantService, IEmployeeService employeeService, IControllerRuntime runtime) :
        base(tenantService, employeeService, runtime, new EmployeeMap())
    {
    }
}