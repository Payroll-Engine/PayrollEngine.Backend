using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the report parameters
/// </summary>
public abstract class ReportParameterController : RepositoryChildObjectController<IReportService, IReportParameterService,
    IReportRepository, IReportParameterRepository,
    DomainObject.Report, DomainObject.ReportParameter, ApiObject.ReportParameter>
{
    protected ReportParameterController(IReportService reportService, IReportParameterService reportParameterService,
        IControllerRuntime runtime) :
        base(reportService, reportParameterService, runtime, new ReportParameterMap())
    {
    }
}