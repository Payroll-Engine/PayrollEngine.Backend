using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollContextService
{
    ITenantService TenantService { get; set; }
    ILookupSetService RegulationLookupSetService { get; set; }
    IPayrollService PayrollService { get; set; }
    IRegulationService RegulationService { get; set; }
    ICaseService CaseService { get; set; }
    ICaseFieldService CaseFieldService { get; set; }
    ICaseRelationService CaseRelationService { get; set; }
    IUserService UserService { get; set; }
    ITaskService TaskService { get; set; }
    ILogService LogService { get; set; }
    IGlobalCaseChangeService GlobalChangeService { get; set; }
    IGlobalCaseValueService GlobalCaseValueService { get; set; }
    INationalCaseChangeService NationalChangeService { get; set; }
    INationalCaseValueService NationalCaseValueService { get; set; }
    ICompanyCaseChangeService CompanyChangeService { get; set; }
    ICompanyCaseValueService CompanyCaseValueService { get; set; }
    IEmployeeService EmployeeService { get; set; }
    IEmployeeCaseChangeService EmployeeChangeService { get; set; }
    IEmployeeCaseValueService EmployeeCaseValueService { get; set; }
    IWebhookDispatchService WebhookDispatchService { get; set; }
    IPayrollCalculatorProvider PayrollCalculatorProvider { get; set; }
}