using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public class PayrunJobServiceSettings
{
    public ILookupSetRepository RegulationLookupSetRepository { get; init; }
    public ICalendarRepository CalendarRepository { get; init; }
    public IUserRepository UserRepository { get; init; }
    public ITaskRepository TaskRepository { get; init; }
    public ILogRepository LogRepository { get; init; }
    public IDivisionRepository DivisionRepository { get; init; }
    public IEmployeeRepository EmployeeRepository { get; init; }
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; init; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; init; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; init; }
    public IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; init; }
    public IPayrunRepository PayrunRepository { get; init; }
    public IPayrunJobRepository PayrunJobRepository { get; init; }
    public IRegulationRepository RegulationRepository { get; init; }
    public IRegulationShareRepository RegulationShareRepository { get; init; }
    public IPayrollRepository PayrollRepository { get; init; }
    public IPayrollResultRepository PayrollResultRepository { get; init; }
    public IPayrollConsolidatedResultRepository PayrollConsolidatedResultRepository { get; init; }
    public IPayrollResultSetRepository PayrollResultSetRepository { get; init; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }
}