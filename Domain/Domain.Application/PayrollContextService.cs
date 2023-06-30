using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application;

public class PayrollContextService : IPayrollContextService
{
    public ITenantService TenantService { get; init; }
    public ICalendarService CalendarService { get; init; }
    public ILookupSetService RegulationLookupSetService { get; init; }
    public IPayrollService PayrollService { get; init; }
    public IDivisionService DivisionService { get; init; }
    public IRegulationService RegulationService { get; init; }
    public ICaseService CaseService { get; init; }
    public ICaseFieldService CaseFieldService { get; init; }
    public IUserService UserService { get; init; }
    public ITaskService TaskService { get; init; }
    public ILogService LogService { get; init; }
    public IGlobalCaseChangeService GlobalChangeService { get; init; }
    public IGlobalCaseValueService GlobalCaseValueService { get; init; }
    public INationalCaseChangeService NationalChangeService { get; init; }
    public INationalCaseValueService NationalCaseValueService { get; init; }
    public ICompanyCaseChangeService CompanyChangeService { get; init; }
    public ICompanyCaseValueService CompanyCaseValueService { get; init; }
    public IEmployeeService EmployeeService { get; init; }
    public IEmployeeCaseChangeService EmployeeChangeService { get; init; }
    public IEmployeeCaseValueService EmployeeCaseValueService { get; init; }
    public IWebhookDispatchService WebhookDispatchService { get; init; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }
}