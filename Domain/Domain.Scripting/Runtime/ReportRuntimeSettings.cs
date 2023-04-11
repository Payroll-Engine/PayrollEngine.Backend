using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class ReportRuntimeSettings : RuntimeSettings
{
    /// <summary>The query service</summary>
    public IQueryService QueryService { get; set; }

    /// <summary>The api controller context</summary>
    public IApiControllerContext ControllerContext { get; set; }

    /// <summary>The global case value repository</summary>
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; set; }

    /// <summary>The national case value repository</summary>
    public INationalCaseValueRepository NationalCaseValueRepository { get; set; }

    /// <summary>The company case value repository</summary>
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; set; }

    /// <summary>The employee case value repository</summary>
    public IEmployeeCaseValueRepository EmployeCaseValueRepository { get; set; }

    /// <summary>The lookup repository</summary>
    public ILookupRepository LookupRepository { get; set; }

    /// <summary>The lookup repository</summary>
    public ILookupValueRepository LookupValueRepository { get; set; }

    /// <summary>The collector repository</summary>
    public ICollectorRepository CollectorRepository { get; set; }

    /// <summary>The wage type repository</summary>
    public IWageTypeRepository WageTypeRepository { get; set; }

    /// <summary>The report log repository</summary>
    public IReportLogRepository ReportLogRepository { get; set; }

    /// <summary>The payroll result repository</summary>
    public IPayrollResultRepository PayrollResultRepository { get; set; }

    /// <summary>The wage type result repository</summary>
    public IWageTypeResultRepository WageTypeResultRepository { get; set; }

    /// <summary>The wage type custom result repository</summary>
    public IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; set; }

    /// <summary>The collector result repository</summary>
    public ICollectorResultRepository CollectorResultRepository { get; set; }

    /// <summary>The collector custom result repository</summary>
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; set; }

    /// <summary>The payrun result repository</summary>
    public IPayrunResultRepository PayrunResultRepository { get; set; }

    /// <summary>The webhook dispatch service</summary>
    public IWebhookDispatchService WebhookDispatchService { get; set; }

    /// <summary>The report</summary>
    public ReportSet Report { get; set; }

    /// <summary>The report request</summary>
    public ReportRequest ReportRequest { get; set; }
}