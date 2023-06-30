using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Setting for the <see cref="ReportTool"/>
/// </summary>
public class ReportToolSettings : FunctionToolSettings
{
    public IWebhookDispatchService WebhookDispatchService { get; init; }

    public IUserRepository UserRepository { get; init; }
    public IEmployeeRepository EmployeeRepository { get; init; }
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; init; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; init; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; init; }
    public IEmployeeCaseValueRepository EmployeCaseValueRepository { get; init; }
    public IRegulationRepository RegulationRepository { get; init; }
    public ILookupRepository LookupRepository { get; init; }
    public ILookupValueRepository LookupValueRepository { get; init; }
    public IWageTypeRepository WageTypeRepository { get; init; }
    public IReportLogRepository ReportLogRepository { get; init; }
    public IPayrollRepository PayrollRepository { get; init; }
    public IPayrollResultRepository PayrollResultRepository { get; init; }
    public IWageTypeResultRepository WageTypeResultRepository { get; init; }
    public IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; init; }
    public ICollectorResultRepository CollectorResultRepository { get; init; }
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; init; }
    public IPayrunResultRepository PayrunResultRepository { get; init; }
    public IPayrunRepository PayrunRepository { get; init; }
    public IReportSetRepository ReportRepository { get; init; }
    public IWebhookRepository WebhookRepository { get; init; }
}