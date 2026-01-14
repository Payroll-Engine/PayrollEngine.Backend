using System;
using Microsoft.Extensions.Configuration;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Api.Core;

internal static class ApiServiceFactory
{
    // services setup
    internal static void SetupApiServices(IServiceCollection services, IConfiguration configuration)
    {
        // system services
        services.AddScoped(NewRegulationShareService);

        // services
        TenantServiceFactory.SetupServices(services, configuration);
        RegulationServiceFactory.SetupServices(services);
        PayrollServiceFactory.SetupServices(services);
        CaseValueServiceFactory.SetupServices(services);
        PayrunServiceFactory.SetupServices(services);
        ReportServiceFactory.SetupServices(services);
    }

    #region Service Factories

    private static IRegulationShareService NewRegulationShareService(IServiceProvider serviceProvider) =>
        new RegulationShareService(serviceProvider.GetRequiredService<IRegulationShareRepository>());

    private static class TenantServiceFactory
    {
        private static IConfiguration Configuration { get; set; }

        // services setup
        internal static void SetupServices(IServiceCollection services, IConfiguration configuration)
        {
            Configuration = configuration;

            // webhook timeout
            WebhookDispatchService.Timeout = GetWebhookTimeout();

            services.AddScoped(NewTenantService);
            services.AddScoped(NewCalendarService);
            services.AddScoped(NewWebhookService);
            services.AddScoped(NewWebhookMessageService);
            services.AddScoped(NewWebhookDispatchService);
            services.AddScoped(NewUserService);
            services.AddScoped(NewDivisionService);
            services.AddScoped(NewTaskService);
            services.AddScoped(NewLogService);
            services.AddScoped(NewReportLogService);
            services.AddScoped(NewEmployeeService);
        }

        private static ITenantService NewTenantService(IServiceProvider serviceProvider) =>
            new TenantService(
                serviceProvider.GetRequiredService<ITenantRepository>());

        private static ICalendarService NewCalendarService(IServiceProvider serviceProvider) =>
            new CalendarService(
                serviceProvider.GetRequiredService<ICalendarRepository>());

        private static IWebhookService NewWebhookService(IServiceProvider serviceProvider) =>
            new WebhookService(serviceProvider.GetRequiredService<IWebhookRepository>());

        private static IWebhookMessageService NewWebhookMessageService(IServiceProvider serviceProvider) =>
            new WebhookMessageService(serviceProvider.GetRequiredService<IWebhookMessageRepository>());

        private static IWebhookDispatchService NewWebhookDispatchService(IServiceProvider serviceProvider)
        {
            return new WebhookDispatchService(
                serviceProvider.GetRequiredService<ITenantRepository>(),
                serviceProvider.GetRequiredService<IUserRepository>(),
                serviceProvider.GetRequiredService<IWebhookRepository>(),
                serviceProvider.GetRequiredService<IWebhookMessageRepository>());
        }

        private static TimeSpan GetWebhookTimeout()
        {
            var timeout = TimeSpan.FromMinutes(1);
            if (Configuration != null)
            {
                var serverConfiguration = Configuration.GetConfiguration<PayrollServerConfiguration>();
                timeout = serverConfiguration.WebhookTimeout;
            }
            return timeout;
        }

        private static IUserService NewUserService(IServiceProvider serviceProvider) =>
            new UserService(serviceProvider.GetRequiredService<IUserRepository>());

        private static IDivisionService NewDivisionService(IServiceProvider serviceProvider) =>
            new DivisionService(serviceProvider.GetRequiredService<IDivisionRepository>());

        private static ITaskService NewTaskService(IServiceProvider serviceProvider) =>
            new TaskService(serviceProvider.GetRequiredService<ITaskRepository>());

        private static ILogService NewLogService(IServiceProvider serviceProvider) =>
            new LogService(serviceProvider.GetRequiredService<ILogRepository>());

        private static IReportLogService NewReportLogService(IServiceProvider serviceProvider) =>
            new ReportLogService(
                serviceProvider.GetRequiredService<IReportLogRepository>());

        private static IEmployeeService NewEmployeeService(IServiceProvider serviceProvider) =>
            new EmployeeService(serviceProvider.GetRequiredService<IEmployeeRepository>());
    }

    private static class RegulationServiceFactory
    {
        // services setup
        internal static void SetupServices(IServiceCollection services)
        {
            services.AddScoped(NewRegulationService);
            services.AddScoped(NewCaseService);
            services.AddScoped(NewCaseAuditService);
            services.AddScoped(NewCaseFieldService);
            services.AddScoped(NewCaseFieldAuditService);
            services.AddScoped(NewCaseRelationService);
            services.AddScoped(NewCaseRelationAuditService);
            services.AddScoped(NewWageTypeService);
            services.AddScoped(NewWageTypeAuditService);
            services.AddScoped(NewCollectorService);
            services.AddScoped(NewCollectorAuditService);
            services.AddScoped(NewLookupService);
            services.AddScoped(NewLookupAuditService);
            services.AddScoped(NewLookupValueService);
            services.AddScoped(NewLookupValueAuditService);
            services.AddScoped(NewLookupSetService);
            services.AddScoped(NewScriptService);
            services.AddScoped(NewScriptAuditService);
        }

        private static IRegulationService NewRegulationService(IServiceProvider serviceProvider) =>
            new RegulationService(serviceProvider.GetRequiredService<IRegulationRepository>());

        private static ICaseService NewCaseService(IServiceProvider serviceProvider) =>
            new CaseService(serviceProvider.GetRequiredService<ICaseRepository>());

        private static ICaseAuditService NewCaseAuditService(IServiceProvider serviceProvider) =>
            new CaseAuditService(serviceProvider.GetRequiredService<ICaseAuditRepository>());

        private static ICaseFieldService NewCaseFieldService(IServiceProvider serviceProvider) =>
            new CaseFieldService(serviceProvider.GetRequiredService<ICaseFieldRepository>());

        private static ICaseFieldAuditService NewCaseFieldAuditService(IServiceProvider serviceProvider) =>
            new CaseFieldAuditService(serviceProvider.GetRequiredService<ICaseFieldAuditRepository>());

        private static ICaseRelationService NewCaseRelationService(IServiceProvider serviceProvider) =>
            new CaseRelationService(serviceProvider.GetRequiredService<ICaseRelationRepository>());

        private static ICaseRelationAuditService NewCaseRelationAuditService(IServiceProvider serviceProvider) =>
            new CaseRelationAuditService(serviceProvider.GetRequiredService<ICaseRelationAuditRepository>());

        private static IWageTypeService NewWageTypeService(IServiceProvider serviceProvider) =>
            new WageTypeService(serviceProvider.GetRequiredService<IWageTypeRepository>());

        private static IWageTypeAuditService NewWageTypeAuditService(IServiceProvider serviceProvider) =>
            new WageTypeAuditService(serviceProvider.GetRequiredService<IWageTypeAuditRepository>());

        private static ICollectorService NewCollectorService(IServiceProvider serviceProvider) =>
            new CollectorService(serviceProvider.GetRequiredService<ICollectorRepository>());

        private static ICollectorAuditService NewCollectorAuditService(IServiceProvider serviceProvider) =>
            new CollectorAuditService(serviceProvider.GetRequiredService<ICollectorAuditRepository>());

        private static ILookupValueAuditService NewLookupValueAuditService(IServiceProvider serviceProvider) =>
            new LookupValueAuditService(serviceProvider.GetRequiredService<ILookupValueAuditRepository>());

        private static ILookupAuditService NewLookupAuditService(IServiceProvider serviceProvider) =>
            new LookupAuditService(serviceProvider.GetRequiredService<ILookupAuditRepository>());

        private static IScriptService NewScriptService(IServiceProvider serviceProvider) =>
            new ScriptService(serviceProvider.GetRequiredService<IScriptRepository>());

        private static IScriptAuditService NewScriptAuditService(IServiceProvider serviceProvider) =>
            new ScriptAuditService(serviceProvider.GetRequiredService<IScriptAuditRepository>());

        private static ILookupService NewLookupService(IServiceProvider serviceProvider) =>
            new LookupService(serviceProvider.GetRequiredService<ILookupRepository>());

        private static ILookupValueService NewLookupValueService(IServiceProvider serviceProvider) =>
            new LookupValueService(serviceProvider.GetRequiredService<ILookupValueRepository>());

        private static ILookupSetService NewLookupSetService(IServiceProvider serviceProvider) =>
            new LookupSetService(serviceProvider.GetRequiredService<ILookupSetRepository>());
    }

    private static class PayrollServiceFactory
    {
        // services setup
        internal static void SetupServices(IServiceCollection services)
        {
            services.AddScoped(NewPayrollService);
            services.AddScoped(NewPayrollContextService);
            services.AddScoped(NewPayrollLayerService);
        }

        private static IPayrollService NewPayrollService(IServiceProvider serviceProvider) =>
            new PayrollService(
                serviceProvider.GetRequiredService<IPayrollRepository>(),
                serviceProvider.GetRequiredService<IGlobalCaseValueRepository>(),
                serviceProvider.GetRequiredService<INationalCaseValueRepository>(),
                serviceProvider.GetRequiredService<ICompanyCaseValueRepository>(),
                serviceProvider.GetRequiredService<IEmployeeCaseValueRepository>());

        private static IPayrollContextService NewPayrollContextService(IServiceProvider serviceProvider) =>
            new PayrollContextService
            {
                TenantService = serviceProvider.GetRequiredService<ITenantService>(),
                RegulationLookupSetService = serviceProvider.GetRequiredService<ILookupSetService>(),
                PayrollService = serviceProvider.GetRequiredService<IPayrollService>(),
                DivisionService = serviceProvider.GetRequiredService<IDivisionService>(),
                RegulationService = serviceProvider.GetRequiredService<IRegulationService>(),
                CaseService = serviceProvider.GetRequiredService<ICaseService>(),
                CaseFieldService = serviceProvider.GetRequiredService<ICaseFieldService>(),
                UserService = serviceProvider.GetRequiredService<IUserService>(),
                TaskService = serviceProvider.GetRequiredService<ITaskService>(),
                LogService = serviceProvider.GetRequiredService<ILogService>(),
                GlobalChangeService = serviceProvider.GetRequiredService<IGlobalCaseChangeService>(),
                GlobalCaseValueService = serviceProvider.GetRequiredService<IGlobalCaseValueService>(),
                NationalChangeService = serviceProvider.GetRequiredService<INationalCaseChangeService>(),
                NationalCaseValueService = serviceProvider.GetRequiredService<INationalCaseValueService>(),
                CompanyChangeService = serviceProvider.GetRequiredService<ICompanyCaseChangeService>(),
                CompanyCaseValueService = serviceProvider.GetRequiredService<ICompanyCaseValueService>(),
                EmployeeService = serviceProvider.GetRequiredService<IEmployeeService>(),
                EmployeeChangeService = serviceProvider.GetRequiredService<IEmployeeCaseChangeService>(),
                EmployeeCaseValueService = serviceProvider.GetRequiredService<IEmployeeCaseValueService>(),
                CalendarService = serviceProvider.GetRequiredService<ICalendarService>(),
                PayrollCalculatorProvider = serviceProvider.GetRequiredService<IPayrollCalculatorProvider>(),
                WebhookDispatchService = serviceProvider.GetRequiredService<IWebhookDispatchService>()
            };

        private static IPayrollLayerService NewPayrollLayerService(IServiceProvider serviceProvider) =>
            new PayrollLayerService(serviceProvider.GetRequiredService<IPayrollLayerRepository>());
    }

    private static class CaseValueServiceFactory
    {
        // services setup
        internal static void SetupServices(IServiceCollection services)
        {
            services.AddScoped(NewGlobalCaseValueService);
            services.AddScoped(NewGlobalCaseDocumentService);
            services.AddScoped(NewGlobalCaseChangeService);

            services.AddScoped(NewNationalCaseValueService);
            services.AddScoped(NewNationalCaseDocumentService);
            services.AddScoped(NewNationalCaseChangeService);

            services.AddScoped(NewCompanyCaseValueService);
            services.AddScoped(NewCompanyCaseDocumentService);
            services.AddScoped(NewCompanyCaseChangeService);

            services.AddScoped(NewEmployeeCaseValueService);
            services.AddScoped(NewEmployeeCaseDocumentService);
            services.AddScoped(NewEmployeeCaseChangeService);
        }

        private static IGlobalCaseValueService NewGlobalCaseValueService(IServiceProvider serviceProvider) =>
            new GlobalCaseValueService(
                serviceProvider.GetRequiredService<IGlobalCaseValueRepository>());

        private static IGlobalCaseDocumentService NewGlobalCaseDocumentService(IServiceProvider serviceProvider) =>
            new GlobalCaseDocumentService(
                serviceProvider.GetRequiredService<IGlobalCaseDocumentRepository>());

        private static IGlobalCaseChangeService NewGlobalCaseChangeService(IServiceProvider serviceProvider) =>
            new GlobalCaseChangeService(
                serviceProvider.GetRequiredService<IWebhookDispatchService>(),
                serviceProvider.GetRequiredService<IGlobalCaseChangeRepository>());

        private static INationalCaseValueService NewNationalCaseValueService(IServiceProvider serviceProvider) =>
            new NationalCaseValueService(
                serviceProvider.GetRequiredService<INationalCaseValueRepository>());

        private static INationalCaseDocumentService NewNationalCaseDocumentService(IServiceProvider serviceProvider) =>
            new NationalCaseDocumentService(
                serviceProvider.GetRequiredService<INationalCaseDocumentRepository>());

        private static INationalCaseChangeService NewNationalCaseChangeService(IServiceProvider serviceProvider) =>
            new NationalCaseChangeService(
                serviceProvider.GetRequiredService<IWebhookDispatchService>(),
                serviceProvider.GetRequiredService<INationalCaseChangeRepository>());

        private static ICompanyCaseValueService NewCompanyCaseValueService(IServiceProvider serviceProvider) =>
            new CompanyCaseValueService(
                serviceProvider.GetRequiredService<ICompanyCaseValueRepository>());

        private static ICompanyCaseDocumentService NewCompanyCaseDocumentService(IServiceProvider serviceProvider) =>
            new CompanyCaseDocumentService(
                serviceProvider.GetRequiredService<ICompanyCaseDocumentRepository>());

        private static ICompanyCaseChangeService NewCompanyCaseChangeService(IServiceProvider serviceProvider) =>
            new CompanyCaseChangeService(
                serviceProvider.GetRequiredService<IWebhookDispatchService>(),
                serviceProvider.GetRequiredService<ICompanyCaseChangeRepository>());

        private static IEmployeeCaseValueService NewEmployeeCaseValueService(IServiceProvider serviceProvider) =>
            new EmployeeCaseValueService(
                serviceProvider.GetRequiredService<IEmployeeCaseValueRepository>());

        private static IEmployeeCaseDocumentService NewEmployeeCaseDocumentService(IServiceProvider serviceProvider) =>
            new EmployeeCaseDocumentService(
                serviceProvider.GetRequiredService<IEmployeeCaseDocumentRepository>());

        private static IEmployeeCaseChangeService NewEmployeeCaseChangeService(IServiceProvider serviceProvider) =>
            new EmployeeCaseChangeService(
                serviceProvider.GetRequiredService<IWebhookDispatchService>(),
                serviceProvider.GetRequiredService<IEmployeeCaseChangeRepository>());
    }

    private static class PayrunServiceFactory
    {
        // services setup
        internal static void SetupServices(IServiceCollection services)
        {
            services.AddScoped(NewPayrunService);
            services.AddScoped(NewPayrunParameterService);
            services.AddScoped(NewPayrunJobService);
            services.AddScoped(NewPayrollResultService);
            services.AddScoped(NewPayrollResultContextService);
            services.AddScoped(NewPayrollConsolidatedResultService);
        }

        private static IPayrunService NewPayrunService(IServiceProvider serviceProvider) =>
            new PayrunService(
                serviceProvider.GetRequiredService<IPayrunRepository>());

        private static IPayrunParameterService NewPayrunParameterService(IServiceProvider serviceProvider) =>
            new PayrunParameterService(
                serviceProvider.GetRequiredService<IPayrunParameterRepository>());

        private static IPayrunJobService NewPayrunJobService(IServiceProvider serviceProvider) =>
            new PayrunJobService(new()
            {
                CalendarRepository = serviceProvider.GetRequiredService<ICalendarRepository>(),
                UserRepository = serviceProvider.GetRequiredService<IUserRepository>(),
                DivisionRepository = serviceProvider.GetRequiredService<IDivisionRepository>(),
                PayrunRepository = serviceProvider.GetRequiredService<IPayrunRepository>(),
                PayrunJobRepository = serviceProvider.GetRequiredService<IPayrunJobRepository>(),
                PayrollRepository = serviceProvider.GetRequiredService<IPayrollRepository>(),
                PayrollCalculatorProvider = serviceProvider.GetRequiredService<IPayrollCalculatorProvider>()
            });

        private static IPayrollResultService NewPayrollResultService(IServiceProvider serviceProvider) =>
            new PayrollResultService(
                serviceProvider.GetRequiredService<IPayrollResultContextService>());

        private static IPayrollResultContextService NewPayrollResultContextService(IServiceProvider serviceProvider) =>
            new PayrollResultContextService
            {
                ResultRepository = serviceProvider.GetRequiredService<IPayrollResultRepository>(),
                CollectorResultRepository = serviceProvider.GetRequiredService<ICollectorResultRepository>(),
                CollectorCustomResultRepository = serviceProvider.GetRequiredService<ICollectorCustomResultRepository>(),
                WageTypeResultRepository = serviceProvider.GetRequiredService<IWageTypeResultRepository>(),
                WageTypeCustomResultRepository = serviceProvider.GetRequiredService<IWageTypeCustomResultRepository>(),
                PayrunResultRepository = serviceProvider.GetRequiredService<IPayrunResultRepository>(),
                ResultSetRepository = serviceProvider.GetRequiredService<IPayrollResultSetRepository>(),
                ConsolidatedResultRepository = serviceProvider.GetRequiredService<IPayrollConsolidatedResultRepository>()
            };

        private static IPayrollConsolidatedResultService NewPayrollConsolidatedResultService(IServiceProvider serviceProvider) =>
            new PayrollConsolidatedResultService(
                serviceProvider.GetRequiredService<IPayrollResultContextService>());
    }

    private static class ReportServiceFactory
    {
        // services setup
        internal static void SetupServices(IServiceCollection services)
        {
            services.AddScoped(NewReportService);
            services.AddScoped(NewReportAuditService);
            services.AddScoped(NewReportSetService);
            services.AddScoped(NewReportParameterService);
            services.AddScoped(NewReportParameterAuditService);
            services.AddScoped(NewReportTemplateService);
            services.AddScoped(NewReportTemplateAuditService);
        }

        private static IReportService NewReportService(IServiceProvider serviceProvider) =>
            new ReportService(
                serviceProvider.GetRequiredService<IReportRepository>(),
                serviceProvider.GetRequiredService<IQueryService>());

        private static IReportAuditService NewReportAuditService(IServiceProvider serviceProvider) =>
            new ReportAuditService(
                serviceProvider.GetRequiredService<IReportAuditRepository>());

        private static IReportSetService NewReportSetService(IServiceProvider serviceProvider) =>
            new ReportSetService(
                serviceProvider.GetRequiredService<IReportSetRepository>(),
                serviceProvider.GetRequiredService<IQueryService>(),
                new()
                {
                    DbContext = serviceProvider.GetRequiredService<IDbContext>(),
                    UserRepository = serviceProvider.GetRequiredService<IUserRepository>(),
                    TaskRepository = serviceProvider.GetRequiredService<ITaskRepository>(),
                    LogRepository = serviceProvider.GetRequiredService<ILogRepository>(),
                    ReportLogRepository = serviceProvider.GetRequiredService<IReportLogRepository>(),
                    DivisionRepository = serviceProvider.GetRequiredService<IDivisionRepository>(),
                    EmployeeRepository = serviceProvider.GetRequiredService<IEmployeeRepository>(),
                    GlobalCaseValueRepository = serviceProvider.GetRequiredService<IGlobalCaseValueRepository>(),
                    NationalCaseValueRepository = serviceProvider.GetRequiredService<INationalCaseValueRepository>(),
                    CompanyCaseValueRepository = serviceProvider.GetRequiredService<ICompanyCaseValueRepository>(),
                    EmployeeCaseValueRepository = serviceProvider.GetRequiredService<IEmployeeCaseValueRepository>(),
                    RegulationRepository = serviceProvider.GetRequiredService<IRegulationRepository>(),
                    LookupRepository = serviceProvider.GetRequiredService<ILookupRepository>(),
                    LookupValueRepository = serviceProvider.GetRequiredService<ILookupValueRepository>(),
                    WageTypeRepository = serviceProvider.GetRequiredService<IWageTypeRepository>(),
                    PayrollRepository = serviceProvider.GetRequiredService<IPayrollRepository>(),
                    PayrollResultRepository = serviceProvider.GetRequiredService<IPayrollResultRepository>(),
                    WageTypeResultRepository = serviceProvider.GetRequiredService<IWageTypeResultRepository>(),
                    WageTypeCustomResultRepository = serviceProvider.GetRequiredService<IWageTypeCustomResultRepository>(),
                    CollectorResultRepository = serviceProvider.GetRequiredService<ICollectorResultRepository>(),
                    CollectorCustomResultRepository = serviceProvider.GetRequiredService<ICollectorCustomResultRepository>(),
                    PayrunResultRepository = serviceProvider.GetRequiredService<IPayrunResultRepository>(),
                    PayrunRepository = serviceProvider.GetRequiredService<IPayrunRepository>(),
                    ReportRepository = serviceProvider.GetRequiredService<IReportSetRepository>(),
                    WebhookRepository = serviceProvider.GetRequiredService<IWebhookRepository>(),
                    CalendarRepository = serviceProvider.GetRequiredService<ICalendarRepository>(),
                    PayrollCalculatorProvider = serviceProvider.GetRequiredService<IPayrollCalculatorProvider>(),
                    WebhookDispatchService = serviceProvider.GetRequiredService<IWebhookDispatchService>()
                });

        private static IReportParameterService NewReportParameterService(IServiceProvider serviceProvider) =>
            new ReportParameterService(
                serviceProvider.GetRequiredService<IReportParameterRepository>());

        private static IReportParameterAuditService NewReportParameterAuditService(IServiceProvider serviceProvider) =>
            new ReportParameterAuditService(
                serviceProvider.GetRequiredService<IReportParameterAuditRepository>());

        private static IReportTemplateService NewReportTemplateService(IServiceProvider serviceProvider) =>
            new ReportTemplateService(
                serviceProvider.GetRequiredService<IReportTemplateRepository>());

        private static IReportTemplateAuditService NewReportTemplateAuditService(IServiceProvider serviceProvider) =>
            new ReportTemplateAuditService(
                serviceProvider.GetRequiredService<IReportTemplateAuditRepository>());
    }

    #endregion
}