using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Api.Core;
using PayrollEngine.Backend.Controller;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Backend.Server;

/// <summary>
/// Startup extension
/// </summary>
public static class ProviderStartupExtensions
{
    /// <summary>
    /// Adds the local API services
    /// </summary>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddLocalApiServices(this IServiceCollection services)
    {
        // case controllers
        services.AddTransient(ctx => new CaseAuditController(
            ctx.GetRequiredService<ICaseService>(),
            ctx.GetRequiredService<ICaseAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CaseController(
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<ICaseService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CaseFieldAuditController(
            ctx.GetRequiredService<ICaseFieldService>(),
            ctx.GetRequiredService<ICaseFieldAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CaseFieldController(
            ctx.GetRequiredService<ICaseService>(),
            ctx.GetRequiredService<ICaseFieldService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CaseRelationAuditController(
            ctx.GetRequiredService<ICaseRelationService>(),
            ctx.GetRequiredService<ICaseRelationAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CaseRelationController(
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<ICaseService>(),
            ctx.GetRequiredService<ICaseRelationService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // collector controllers
        services.AddTransient(ctx => new CollectorAuditController(
            ctx.GetRequiredService<ICollectorService>(),
            ctx.GetRequiredService<ICollectorAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CollectorController(
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<ICollectorService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // company controllers
        services.AddTransient(ctx => new CompanyCaseChangeController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<ICompanyCaseChangeService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CompanyCaseValueController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<ICompanyCaseValueService>(),
            ctx.GetRequiredService<IPayrollService>(),
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<ILookupSetService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new CompanyCaseDocumentController(
            ctx.GetRequiredService<ICompanyCaseValueService>(),
            ctx.GetRequiredService<ICompanyCaseDocumentService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // division controllers
        services.AddTransient(ctx => new DivisionController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IDivisionService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // employee controllers
        services.AddTransient(ctx => new EmployeeCaseChangeController(
            ctx.GetRequiredService<IEmployeeService>(),
            ctx.GetRequiredService<IEmployeeCaseChangeService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new EmployeeCaseValueController(
            ctx.GetRequiredService<IEmployeeService>(),
            ctx.GetRequiredService<IEmployeeCaseValueService>(),
            ctx.GetRequiredService<IPayrollService>(),
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<ILookupSetService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new EmployeeCaseDocumentController(
            ctx.GetRequiredService<IEmployeeCaseValueService>(),
            ctx.GetRequiredService<IEmployeeCaseDocumentService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new EmployeeController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IEmployeeService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // lookup controllers
        services.AddTransient(ctx => new LookupAuditController(
            ctx.GetRequiredService<ILookupService>(),
            ctx.GetRequiredService<ILookupAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new LookupController(
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<ILookupService>(),
            ctx.GetRequiredService<ILookupSetService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new LookupValueAuditController(
            ctx.GetRequiredService<ILookupValueService>(),
            ctx.GetRequiredService<ILookupValueAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new LookupValueController(
            ctx.GetRequiredService<ILookupService>(),
            ctx.GetRequiredService<ILookupValueService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // national controllers
        services.AddTransient(ctx => new NationalCaseChangeController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<INationalCaseChangeService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new NationalCaseValueController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IPayrollService>(),
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<INationalCaseValueService>(),
            ctx.GetRequiredService<ILookupSetService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new NationalCaseDocumentController(
            ctx.GetRequiredService<INationalCaseValueService>(),
            ctx.GetRequiredService<INationalCaseDocumentService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // payroll controllers
        services.AddTransient(ctx => new PayrollController(
            ctx.GetRequiredService<IPayrollContextService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new PayrollLayerController(
            ctx.GetRequiredService<IPayrollService>(),
            ctx.GetRequiredService<IPayrollLayerService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new PayrollResultController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IPayrollResultService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new PayrollConsolidatedResultController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IPayrollConsolidatedResultService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // payrun controllers
        services.AddTransient(ctx => new PayrunController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IPayrunService>(),
            ctx.GetRequiredService<IPayrollService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new PayrunParameterController(
            ctx.GetRequiredService<IPayrunService>(),
            ctx.GetRequiredService<IPayrunParameterService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new PayrunJobController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IPayrunJobService>(),
            ctx.GetRequiredService<IWebhookDispatchService>(),
            ctx.GetRequiredService<IPayrunJobQueue>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // regulation controllers
        services.AddTransient(ctx => new RegulationController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<ICaseService>(),
            ctx.GetRequiredService<ICaseFieldService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // report controllers
        services.AddTransient(ctx => new ReportController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<IReportService>(),
            ctx.GetRequiredService<IReportSetService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new ReportParameterController(
            ctx.GetRequiredService<IReportService>(),
            ctx.GetRequiredService<IReportParameterService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // script controllers
        services.AddTransient(ctx => new ScriptAuditController(
            ctx.GetRequiredService<IScriptService>(),
            ctx.GetRequiredService<IScriptAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new ScriptController(
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<IScriptService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // tenant controllers
        services.AddTransient<ITenantService, TenantService>();
        services.AddTransient(ctx => new TenantController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<IRegulationShareService>(),
            ctx.GetRequiredService<IReportService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // calendar controllers
        services.AddTransient(ctx => new CalendarController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<ICalendarService>(),
            ctx.GetRequiredService<IPayrollCalculatorProvider>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // user controllers
        services.AddTransient(ctx => new UserController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IUserService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // wage type controllers
        services.AddTransient(ctx => new WageTypeAuditController(
            ctx.GetRequiredService<IWageTypeService>(),
            ctx.GetRequiredService<IWageTypeAuditService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new WageTypeController(
            ctx.GetRequiredService<IRegulationService>(),
            ctx.GetRequiredService<IWageTypeService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        // webhook controllers
        services.AddTransient(ctx => new WebhookController(
            ctx.GetRequiredService<ITenantService>(),
            ctx.GetRequiredService<IWebhookService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        services.AddTransient(ctx => new WebhookMessageController(
            ctx.GetRequiredService<IWebhookService>(),
            ctx.GetRequiredService<IWebhookMessageService>(),
            ctx.GetRequiredService<IControllerRuntime>()));

        return services;
    }
}