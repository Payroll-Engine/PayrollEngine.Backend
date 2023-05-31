using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation report parameter audits
/// </summary>
public abstract class ReportParameterAuditController : RepositoryChildObjectController<IReportParameterService, IReportParameterAuditService,
    IReportParameterRepository, IReportParameterAuditRepository,
    DomainObject.ReportParameter, DomainObject.ReportParameterAudit, ApiObject.ReportParameterAudit>
{
    protected ReportParameterAuditController(IReportParameterService reportParameterService, IReportParameterAuditService auditService, IControllerRuntime runtime) :
        base(reportParameterService, auditService, runtime, new ReportParameterAuditMap())
    {
    }
}