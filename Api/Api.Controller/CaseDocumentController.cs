﻿using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the case documents
/// </summary>
public abstract class CaseDocumentController<TParentService, TParentRepo, TRepo, TParent> :
    RepositoryChildObjectController<TParentService, ICaseDocumentService<TRepo, DomainObject.CaseDocument>, TParentRepo, TRepo, TParent, DomainObject.CaseDocument, ApiObject.CaseDocument>
    where TParentService : class, IRepositoryApplicationService<TParentRepo>
    where TParentRepo : class, IDomainRepository
    where TRepo : class, IChildDomainRepository<DomainObject.CaseDocument>
    where TParent : class, DomainObject.IDomainObject, new()
{
    protected CaseDocumentController(TParentService parentService, ICaseDocumentService<TRepo, DomainObject.CaseDocument> caseDocumentService,
        IControllerRuntime runtime) :
        base(parentService, caseDocumentService, runtime, new CaseDocumentMap())
    {
    }
}