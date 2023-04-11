using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation report audits
/// </summary>
// ReSharper disable StringLiteralTypo
[ApiControllerName("Report audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports/{reportId}/audits")]
// ReSharper restore StringLiteralTypo
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.ReportAudit)]
public abstract class ReportAuditController : RepositoryChildObjectController<IReportService, IReportAuditService,
    IReportRepository, IReportAuditRepository,
    DomainObject.Report, DomainObject.ReportAudit, ApiObject.ReportAudit>
{
    protected ReportAuditController(IReportService reportService, IReportAuditService reportAuditService, IControllerRuntime runtime) :
        base(reportService, reportAuditService, runtime, new ReportAuditMap())
    {
    }
}