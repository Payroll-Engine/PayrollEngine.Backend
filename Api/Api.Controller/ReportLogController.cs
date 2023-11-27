using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the report logs
/// </summary>
public abstract class ReportLogController(ITenantService tenantService, IReportLogService reportLogService,
        IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, IReportLogService,
    ITenantRepository, IReportLogRepository,
    DomainObject.Tenant, DomainObject.ReportLog, ApiObject.ReportLog>(tenantService, reportLogService, runtime, new ReportLogMap());