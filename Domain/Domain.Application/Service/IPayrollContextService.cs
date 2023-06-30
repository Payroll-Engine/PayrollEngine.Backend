using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollContextService
{
    ITenantService TenantService { get; }
    IPayrollCalculatorProvider PayrollCalculatorProvider { get; }

    ICalendarService CalendarService { get; init; }
    ILookupSetService RegulationLookupSetService { get; init; }
    IPayrollService PayrollService { get; }
    IDivisionService DivisionService { get; init; }
    IRegulationService RegulationService { get; init; }
    ICaseService CaseService { get; init; }
    ICaseFieldService CaseFieldService { get; init; }
    IUserService UserService { get; init; }
    ITaskService TaskService { get; init; }
    ILogService LogService { get; init; }
    IGlobalCaseChangeService GlobalChangeService { get; init; }
    IGlobalCaseValueService GlobalCaseValueService { get; init; }
    INationalCaseChangeService NationalChangeService { get; init; }
    INationalCaseValueService NationalCaseValueService { get; init; }
    ICompanyCaseChangeService CompanyChangeService { get; init; }
    ICompanyCaseValueService CompanyCaseValueService { get; init; }
    IEmployeeService EmployeeService { get; init; }
    IEmployeeCaseChangeService EmployeeChangeService { get; init; }
    IEmployeeCaseValueService EmployeeCaseValueService { get; init; }
    IWebhookDispatchService WebhookDispatchService { get; init; }
}