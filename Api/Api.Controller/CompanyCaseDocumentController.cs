using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll company case documents
/// </summary>
public abstract class CompanyCaseDocumentController(ICompanyCaseValueService caseValueService,
        ICaseDocumentService<ICompanyCaseDocumentRepository,
            DomainObject.CaseDocument> caseDocumentService, IControllerRuntime runtime)
    : CaseDocumentController<ICompanyCaseValueService, ICompanyCaseValueRepository,
    ICompanyCaseDocumentRepository, DomainObject.Tenant>(caseValueService, caseDocumentService, runtime);