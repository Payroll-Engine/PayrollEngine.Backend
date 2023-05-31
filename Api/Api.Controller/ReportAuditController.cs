using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation report audits
/// </summary>
public abstract class ReportAuditController : RepositoryChildObjectController<IReportService, IReportAuditService,
    IReportRepository, IReportAuditRepository,
    DomainObject.Report, DomainObject.ReportAudit, ApiObject.ReportAudit>
{
    protected ReportAuditController(IReportService reportService, IReportAuditService reportAuditService, IControllerRuntime runtime) :
        base(reportService, reportAuditService, runtime, new ReportAuditMap())
    {
    }
}