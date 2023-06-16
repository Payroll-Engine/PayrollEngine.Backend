using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public class PayrunJobServiceSettings
{
    public ILookupSetRepository RegulationLookupSetRepository { get; set; }
    public ICalendarRepository CalendarRepository { get; set; }
    public IUserRepository UserRepository { get; set; }
    public ITaskRepository TaskRepository { get; set; }
    public ILogRepository LogRepository { get; set; }
    public IDivisionRepository DivisionRepository { get; set; }
    public IEmployeeRepository EmployeeRepository { get; set; }
    public ICaseRepository CaseRepository { get; set; }
    public ICaseFieldRepository CaseFieldRepository { get; set; }
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; set; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; set; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; set; }
    public IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; set; }
    public IPayrunRepository PayrunRepository { get; set; }
    public IPayrunJobRepository PayrunJobRepository { get; set; }
    public ICollectorRepository CollectorRepository { get; set; }
    public ICollectorResultRepository CollectorResultRepository { get; set; }
    public IWageTypeRepository WageTypeRepository { get; set; }
    public IWageTypeAuditRepository WageTypeAuditRepository { get; set; }
    public IRegulationRepository RegulationRepository { get; set; }
    public IRegulationShareRepository RegulationShareRepository { get; set; }
    public IPayrollRepository PayrollRepository { get; set; }
    public IPayrollResultRepository PayrollResultRepository { get; set; }
    public IPayrollConsolidatedResultRepository PayrollConsolidatedResultRepository { get; set; }
    public IPayrollResultSetRepository PayrollResultSetRepository { get; set; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; set; }
}