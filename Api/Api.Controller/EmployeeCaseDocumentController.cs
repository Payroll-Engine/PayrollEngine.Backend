using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll employee case documents
/// </summary>
[ApiControllerName("Employee case documents")]
[Route("api/tenants/{tenantId}/employees/{employeeId}/cases/{caseValueId}/documents")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.EmployeeCaseDocument)]
public abstract class EmployeeCaseDocumentController : CaseDocumentController<IEmployeeCaseValueService,
    IEmployeeCaseValueRepository, IEmployeeCaseDocumentRepository,
    DomainObject.Employee>
{
    protected EmployeeCaseDocumentController(IEmployeeCaseValueService caseValueService, ICaseDocumentService<IEmployeeCaseDocumentRepository,
        DomainObject.CaseDocument> caseDocumentService, IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }
}