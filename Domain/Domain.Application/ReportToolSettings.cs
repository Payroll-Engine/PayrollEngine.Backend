using System.Globalization;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Setting for the <see cref="ReportTool"/>
/// </summary>
public class ReportToolSettings : FunctionToolSettings
{
    public CultureInfo Culture { get; set; }

    public IUserRepository UserRepository { get; set; }
    public IEmployeeRepository EmployeeRepository { get; set; }
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; set; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; set; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; set; }
    public IEmployeeCaseValueRepository EmployeCaseValueRepository { get; set; }
    public IRegulationRepository RegulationRepository { get; set; }
    public ILookupRepository LookupRepository { get; set; }
    public ILookupValueRepository LookupValueRepository { get; set; }
    public ICollectorRepository CollectorRepository { get; set; }
    public IWageTypeRepository WageTypeRepository { get; set; }
    public IReportLogRepository ReportLogRepository { get; set; }
    public IPayrollRepository PayrollRepository { get; set; }
    public IPayrollResultRepository PayrollResultRepository { get; set; }
    public IWageTypeResultRepository WageTypeResultRepository { get; set; }
    public IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; set; }
    public ICollectorResultRepository CollectorResultRepository { get; set; }
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; set; }
    public IPayrunResultRepository PayrunResultRepository { get; set; }
    public IPayrunRepository PayrunRepository { get; set; }
    public IReportSetRepository ReportRepository { get; set; }
    public IWebhookRepository WebhookRepository { get; set; }

    public IWebhookDispatchService WebhookDispatchService { get; set; }
}