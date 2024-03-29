﻿using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for divisions
/// </summary>
public abstract class DivisionController(ITenantService tenantService, IDivisionService divisionService,
        IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, IDivisionService,
    ITenantRepository, IDivisionRepository,
    DomainObject.Tenant, DomainObject.Division, ApiObject.Division>(tenantService, divisionService, runtime, new DivisionMap());