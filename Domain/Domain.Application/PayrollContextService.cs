
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Domain.Application;

public class PayrollContextService : IPayrollContextService
{
    public ITenantService TenantService { get; set; }
    public ILookupSetService RegulationLookupSetService { get; set; }
    public IPayrollService PayrollService { get; set; }
    public IRegulationService RegulationService { get; set; }
    public ICaseService CaseService { get; set; }
    public ICaseFieldService CaseFieldService { get; set; }
    public ICaseRelationService CaseRelationService { get; set; }
    public IUserService UserService { get; set; }
    public ITaskService TaskService { get; set; }
    public ILogService LogService { get; set; }
    public IGlobalCaseChangeService GlobalChangeService { get; set; }
    public IGlobalCaseValueService GlobalCaseValueService { get; set; }
    public INationalCaseChangeService NationalChangeService { get; set; }
    public INationalCaseValueService NationalCaseValueService { get; set; }
    public ICompanyCaseChangeService CompanyChangeService { get; set; }
    public ICompanyCaseValueService CompanyCaseValueService { get; set; }
    public IEmployeeService EmployeeService { get; set; }
    public IEmployeeCaseChangeService EmployeeChangeService { get; set; }
    public IEmployeeCaseValueService EmployeeCaseValueService { get; set; }
    public Model.IWebhookDispatchService WebhookDispatchService { get; set; }
}