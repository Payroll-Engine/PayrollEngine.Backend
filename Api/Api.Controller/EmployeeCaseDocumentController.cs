using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll employee case documents
/// </summary>
public abstract class EmployeeCaseDocumentController(IEmployeeCaseValueService caseValueService,
        ICaseDocumentService<IEmployeeCaseDocumentRepository,
            DomainObject.CaseDocument> caseDocumentService, IControllerRuntime runtime)
    : CaseDocumentController<IEmployeeCaseValueService,
    IEmployeeCaseValueRepository, IEmployeeCaseDocumentRepository,
    DomainObject.Employee>(caseValueService, caseDocumentService, runtime);