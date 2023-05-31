﻿using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payrun parameters
/// </summary>
public abstract class PayrunParameterController : RepositoryChildObjectController<IPayrunService, IPayrunParameterService,
    IPayrunRepository, IPayrunParameterRepository,
    DomainObject.Payrun, DomainObject.PayrunParameter, ApiObject.PayrunParameter>
{
    protected PayrunParameterController(IPayrunService payrunService, IPayrunParameterService payrunParameterService,
        IControllerRuntime runtime) :
        base(payrunService, payrunParameterService, runtime, new PayrunParameterMap())
    {
    }
}