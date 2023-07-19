using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class ReportRuntimeSettings : RuntimeSettings
{
    /// <summary>The query service</summary>
    public IQueryService QueryService { get; init; }

    /// <summary>The api controller context</summary>
    public IApiControllerContext ControllerContext { get; init; }

    /// <summary>The global case value repository</summary>
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; init; }

    /// <summary>The national case value repository</summary>
    public INationalCaseValueRepository NationalCaseValueRepository { get; init; }

    /// <summary>The company case value repository</summary>
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; init; }

    /// <summary>The employee case value repository</summary>
    public IEmployeeCaseValueRepository EmployeCaseValueRepository { get; init; }

    /// <summary>The lookup repository</summary>
    public ILookupRepository LookupRepository { get; init; }

    /// <summary>The lookup repository</summary>
    public ILookupValueRepository LookupValueRepository { get; init; }

    /// <summary>The wage type repository</summary>
    public IWageTypeRepository WageTypeRepository { get; init; }

    /// <summary>The report log repository</summary>
    public IReportLogRepository ReportLogRepository { get; init; }

    /// <summary>The payroll result repository</summary>
    public IPayrollResultRepository PayrollResultRepository { get; init; }

    /// <summary>The wage type result repository</summary>
    public IWageTypeResultRepository WageTypeResultRepository { get; init; }

    /// <summary>The wage type custom result repository</summary>
    public IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; init; }

    /// <summary>The collector result repository</summary>
    public ICollectorResultRepository CollectorResultRepository { get; init; }

    /// <summary>The collector custom result repository</summary>
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; init; }

    /// <summary>The payrun result repository</summary>
    public IPayrunResultRepository PayrunResultRepository { get; init; }

    /// <summary>The report</summary>
    public ReportSet Report { get; init; }

    /// <summary>The report request</summary>
    public ReportRequest ReportRequest { get; init; }
}