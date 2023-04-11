using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll company case documents
/// </summary>
[ApiControllerName("Company case documents")]
[Route("api/tenants/{tenantId}/companycases/{caseValueId}/documents")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CompanyCaseDocument)]
public abstract class CompanyCaseDocumentController : CaseDocumentController<ICompanyCaseValueService, ICompanyCaseValueRepository,
    ICompanyCaseDocumentRepository, DomainObject.Tenant>
{
    protected CompanyCaseDocumentController(ICompanyCaseValueService caseValueService, ICaseDocumentService<ICompanyCaseDocumentRepository,
        DomainObject.CaseDocument> caseDocumentService, IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }
}