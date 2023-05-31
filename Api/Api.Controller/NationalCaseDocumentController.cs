﻿using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll national case documents
/// </summary>
public abstract class NationalCaseDocumentController : CaseDocumentController<INationalCaseValueService, INationalCaseValueRepository, INationalCaseDocumentRepository, DomainObject.Tenant>
{
    protected NationalCaseDocumentController(INationalCaseValueService caseValueService, ICaseDocumentService<INationalCaseDocumentRepository, 
        DomainObject.CaseDocument> caseDocumentService, IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }
}