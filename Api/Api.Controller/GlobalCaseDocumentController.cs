using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll global case documents
/// </summary>
[ApiControllerName("Global case documents")]
[Route("api/tenants/{tenantId}/globalcases/{caseValueId}/documents")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.GlobalCaseDocument)]
public abstract class GlobalCaseDocumentController : CaseDocumentController<IGlobalCaseValueService, IGlobalCaseValueRepository, IGlobalCaseDocumentRepository, DomainObject.Tenant>
{
    protected GlobalCaseDocumentController(IGlobalCaseValueService caseValueService, ICaseDocumentService<IGlobalCaseDocumentRepository, 
        DomainObject.CaseDocument> caseDocumentService, IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }
}