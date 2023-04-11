using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation report templates audits
/// </summary>
[ApiControllerName("Report parameter audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports/{reportId}/templates/{parameterId}/audits")]
// ReSharper restore StringLiteralTypo
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.ReportTemplateAudit)]
public abstract class ReportTemplateAuditController : RepositoryChildObjectController<IReportTemplateService, IReportTemplateAuditService,
    IReportTemplateRepository, IReportTemplateAuditRepository,
    DomainObject.ReportTemplate, DomainObject.ReportTemplateAudit, ApiObject.ReportTemplateAudit>
{
    protected ReportTemplateAuditController(IReportTemplateService reportTemplateService, IReportTemplateAuditService reportAuditService, IControllerRuntime runtime) :
        base(reportTemplateService, reportAuditService, runtime, new ReportTemplateAuditMap())
    {
    }
}