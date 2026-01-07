using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence;

namespace PayrollEngine.Api.Core;

internal static class ApiRepositoryFactory
{

    // repositories setup
    internal static void SetupApiServices(IServiceCollection services, IConfiguration configuration)
    {
        // audit trail
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();
        var auditDisabled = serverConfiguration.AuditTrailDisabled;

        // system repositories
        services.AddScoped(_ => NewRegulationShareRepository());
        TenantRepositoryFactory.SetupRepositories(services);
        RegulationRepositoryFactory.SetupRepositories(services, auditDisabled);
        PayrollRepositoryFactory.SetupRepositories(services, auditDisabled);
        CaseValueRepositoryFactory.SetupRepositories(services, auditDisabled);
        PayrunRepositoryFactory.SetupRepositories(services, auditDisabled);
        ReportRepositoryFactory.SetupRepositories(services, auditDisabled);
    }

    private static IRegulationShareRepository NewRegulationShareRepository() =>
        new RegulationShareRepository(
            RegulationRepositoryFactory.NewRegulationRepository());

    #region Repository Factories

    private static class TenantRepositoryFactory
    {
        internal static void SetupRepositories(IServiceCollection services)
        {
            // repositories setup
            services.AddScoped(_ => NewTenantRepository());
            services.AddScoped(_ => NewCalendarRepository());
            services.AddScoped(_ => NewWebhookRepository());
            services.AddScoped(_ => NewWebhookMessageRepository());
            services.AddScoped(_ => NewUserRepository());
            services.AddScoped(_ => NewDivisionRepository());
            services.AddScoped(_ => NewTaskRepository());
            services.AddScoped(_ => NewLogRepository());
            // report repositories
            services.AddScoped(_ => NewReportLogRepository());
            // employee repositories
            services.AddScoped(_ => NewEmployeeDivisionRepository());
            services.AddScoped(_ => NewEmployeeRepository());
        }

        private static ITenantRepository NewTenantRepository() =>
            new TenantRepository();

        private static ICalendarRepository NewCalendarRepository() =>
            new CalendarRepository();

        private static IWebhookRepository NewWebhookRepository() =>
            new WebhookRepository();

        private static IWebhookMessageRepository NewWebhookMessageRepository() =>
            new WebhookMessageRepository();

        private static IUserRepository NewUserRepository() =>
            new UserRepository();

        private static IDivisionRepository NewDivisionRepository() =>
            new DivisionRepository();

        private static ITaskRepository NewTaskRepository() =>
            new TaskRepository();

        private static ILogRepository NewLogRepository() =>
            new LogRepository();

        private static IReportLogRepository NewReportLogRepository() =>
            new ReportLogRepository();

        private static IEmployeeDivisionRepository NewEmployeeDivisionRepository() =>
            new EmployeeDivisionRepository(
                NewDivisionRepository());

        private static IEmployeeRepository NewEmployeeRepository() =>
            new EmployeeRepository(
                NewEmployeeDivisionRepository());
    }

    private static class RegulationRepositoryFactory
    {
        internal static void SetupRepositories(IServiceCollection services, bool auditDisabled)
        {
            // regulation repository
            services.AddScoped(_ => NewRegulationRepository());
            // case repositories
            services.AddScoped(_ => NewCaseRepository(auditDisabled));
            services.AddScoped(_ => NewCaseAuditRepository());
            services.AddScoped(_ => NewCaseFieldRepository(auditDisabled));
            services.AddScoped(_ => NewCaseFieldAuditRepository());
            services.AddScoped(_ => NewCaseRelationRepository(auditDisabled));
            services.AddScoped(_ => NewCaseRelationAuditService());
            // wage type repositories
            services.AddScoped(_ => NewWageTypeRepository(auditDisabled));
            services.AddScoped(_ => NewWageTypeAuditRepository());
            // collector repositories
            services.AddScoped(_ => NewCollectorRepository(auditDisabled));
            services.AddScoped(_ => NewCollectorAuditRepository());
            // lookup repositories
            services.AddScoped(_ => NewLookupRepository(auditDisabled));
            services.AddScoped(_ => NewLookupAuditRepository());
            services.AddScoped(_ => NewLookupValueRepository(auditDisabled));
            services.AddScoped(_ => NewLookupValueAuditRepository());
            services.AddScoped(_ => NewLookupSetRepository(auditDisabled));
            // script repositories
            services.AddScoped(_ => NewScriptRepository(auditDisabled));
            services.AddScoped(_ => NewScriptAuditRepository());
            services.AddScoped(_ => NewScriptProvider());
        }

        internal static IRegulationRepository NewRegulationRepository() =>
            new RegulationRepository();

        private static ICaseAuditRepository NewCaseAuditRepository() =>
            new CaseAuditRepository();

        internal static ICaseFieldRepository NewCaseFieldRepository(bool auditDisabled) =>
            new CaseFieldRepository(
                NewRegulationRepository(),
                NewCaseRepository(auditDisabled),
                NewCaseFieldAuditRepository(),
                auditDisabled);

        private static ICaseFieldAuditRepository NewCaseFieldAuditRepository() =>
            new CaseFieldAuditRepository();

        private static ICaseRelationRepository NewCaseRelationRepository(bool auditDisabled) =>
            new CaseRelationRepository(
                NewRegulationRepository(),
                NewScriptRepository(auditDisabled),
                NewCaseRelationAuditService(),
                auditDisabled);

        private static ICaseRelationAuditRepository NewCaseRelationAuditService() =>
            new CaseRelationAuditRepository();

        private static IWageTypeRepository NewWageTypeRepository(bool auditDisabled) =>
            new WageTypeRepository(
                NewRegulationRepository(),
                NewScriptRepository(auditDisabled),
                NewWageTypeAuditRepository(),
                auditDisabled);

        private static IWageTypeAuditRepository NewWageTypeAuditRepository() =>
            new WageTypeAuditRepository();

        internal static ICaseRepository NewCaseRepository(bool auditDisabled) =>
            new CaseRepository(
                NewRegulationRepository(),
                NewScriptRepository(auditDisabled),
                NewCaseAuditRepository(),
                auditDisabled);

        private static ICollectorAuditRepository NewCollectorAuditRepository() =>
            new CollectorAuditRepository();

        private static ICollectorRepository NewCollectorRepository(bool auditDisabled) =>
            new CollectorRepository(
                NewRegulationRepository(),
                NewScriptRepository(auditDisabled),
                NewCollectorAuditRepository(),
                auditDisabled);

        private static ILookupValueAuditRepository NewLookupValueAuditRepository() =>
            new LookupValueAuditRepository();

        private static ILookupValueRepository NewLookupValueRepository(bool auditDisabled) =>
            new LookupValueRepository(
                NewRegulationRepository(),
                NewLookupValueAuditRepository(),
                auditDisabled);

        private static ILookupAuditRepository NewLookupAuditRepository() =>
            new LookupAuditRepository();

        private static ILookupRepository NewLookupRepository(bool auditDisabled) =>
            new LookupRepository(
                NewRegulationRepository(),
                NewLookupAuditRepository(),
                auditDisabled);

        private static ILookupSetRepository NewLookupSetRepository(bool auditDisabled) =>
            new LookupSetRepository(
                NewRegulationRepository(),
                NewLookupValueRepository(auditDisabled),
                NewLookupAuditRepository(),
                auditDisabled);

        private static IScriptAuditRepository NewScriptAuditRepository() =>
            new ScriptAuditRepository();

        internal static IScriptRepository NewScriptRepository(bool auditDisabled) =>
            new ScriptRepository(
                NewRegulationRepository(),
                NewScriptAuditRepository(),
                auditDisabled);

        private static IScriptProvider NewScriptProvider() =>
            new ScriptProviderRepository();
    }

    private static class PayrollRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, bool auditDisabled)
        {
            services.AddScoped(_ => NewPayrollRepository(auditDisabled));
            services.AddScoped(_ => NewPayrollLayerRepository());
        }

        internal static IPayrollRepository NewPayrollRepository(bool auditDisabled) =>
            new PayrollRepository(
                NewPayrollLayerRepository(),
                RegulationRepositoryFactory.NewRegulationRepository(),
                RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                ReportRepositoryFactory.NewReportSetRepository(auditDisabled),
                RegulationRepositoryFactory.NewScriptRepository(auditDisabled));

        private static IPayrollLayerRepository NewPayrollLayerRepository() =>
            new PayrollLayerRepository();
    }

    private static class CaseValueRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, bool auditDisabled)
        {
            // global case repositories
            services.AddScoped(_ => NewGlobalCaseValueRepository(auditDisabled));
            services.AddScoped(_ => NewGlobalCaseDocumentRepository());
            services.AddScoped(_ => NewGlobalCaseChangeRepository(auditDisabled));
            // national case repositories
            services.AddScoped(_ => NewNationalCaseValueRepository(auditDisabled));
            services.AddScoped(_ => NewNationalCaseDocumentRepository());
            services.AddScoped(_ => NewNationalCaseChangeRepository(auditDisabled));
            // company case repositories
            services.AddScoped(_ => NewCompanyCaseValueRepository(auditDisabled));
            services.AddScoped(_ => NewCompanyCaseDocumentRepository());
            services.AddScoped(_ => NewCompanyCaseChangeRepository(auditDisabled));
            // employee case repositories
            services.AddScoped(_ => NewEmployeeCaseValueRepository(auditDisabled));
            services.AddScoped(_ => NewEmployeeCaseDocumentRepository());
            services.AddScoped(_ => NewEmployeeCaseChangeRepository(auditDisabled));
        }

        private static IGlobalCaseValueRepository NewGlobalCaseValueRepository(bool auditDisabled) =>
            new GlobalCaseValueRepository(RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static IGlobalCaseDocumentRepository NewGlobalCaseDocumentRepository() =>
            new GlobalCaseDocumentRepository();

        private static IGlobalCaseChangeRepository NewGlobalCaseChangeRepository(bool auditDisabled) =>
            new GlobalCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditDisabled),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditDisabled),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                    CaseValueRepository = NewGlobalCaseValueRepository(auditDisabled),
                    CaseValueSetupRepository = new GlobalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                        new GlobalCaseDocumentRepository()),
                    CaseValueChangeRepository = new GlobalCaseValueChangeRepository()
                });

        private static INationalCaseValueRepository NewNationalCaseValueRepository(bool auditDisabled) =>
            new NationalCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static INationalCaseDocumentRepository NewNationalCaseDocumentRepository() =>
            new NationalCaseDocumentRepository();

        private static INationalCaseChangeRepository NewNationalCaseChangeRepository(bool auditDisabled) =>
            new NationalCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditDisabled),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditDisabled),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                    CaseValueRepository = NewNationalCaseValueRepository(auditDisabled),
                    CaseValueSetupRepository = new NationalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                        new NationalCaseDocumentRepository()),
                    CaseValueChangeRepository = new NationalCaseValueChangeRepository()
                });

        private static ICompanyCaseValueRepository NewCompanyCaseValueRepository(bool auditDisabled) =>
            new CompanyCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static ICompanyCaseDocumentRepository NewCompanyCaseDocumentRepository() =>
            new CompanyCaseDocumentRepository();

        private static ICompanyCaseChangeRepository NewCompanyCaseChangeRepository(bool auditDisabled) =>
            new CompanyCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditDisabled),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditDisabled),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                    CaseValueRepository = NewCompanyCaseValueRepository(auditDisabled),
                    CaseValueSetupRepository = new CompanyCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                        new CompanyCaseDocumentRepository()),
                    CaseValueChangeRepository = new CompanyCaseValueChangeRepository()
                });

        private static IEmployeeCaseValueRepository NewEmployeeCaseValueRepository(bool auditDisabled) =>
            new EmployeeCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static IEmployeeCaseDocumentRepository NewEmployeeCaseDocumentRepository() =>
            new EmployeeCaseDocumentRepository();

        private static IEmployeeCaseChangeRepository NewEmployeeCaseChangeRepository(bool auditDisabled) =>
            new EmployeeCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditDisabled),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditDisabled),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                    CaseValueRepository = NewEmployeeCaseValueRepository(auditDisabled),
                    CaseValueSetupRepository = new EmployeeCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled),
                        NewEmployeeCaseDocumentRepository()),
                    CaseValueChangeRepository = new EmployeeCaseValueChangeRepository()
                });
    }

    private static class PayrunRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, bool auditDisabled)
        {
            // payrun repositories
            services.AddScoped(_ => NewPayrunRepository(auditDisabled));
            services.AddScoped(_ => NewPayrunParameterRepository());
            services.AddScoped(_ => NewPayrunJobRepository());
            // collector results repositories
            services.AddScoped(_ => NewCollectorResultRepository());
            services.AddScoped(_ => NewCollectorResultSetRepository());
            services.AddScoped(_ => NewCollectorCustomResultRepository());
            // wage type results repositories
            services.AddScoped(_ => NewWageTypeResultRepository());
            services.AddScoped(_ => NewWageTypeResultSetRepository());
            services.AddScoped(_ => NewWageTypeCustomResultRepository());
            // payrun results repositories
            services.AddScoped(_ => NewPayrunResultRepository());
            // payroll results repositories
            services.AddScoped(_ => NewPayrollResultRepository());
            services.AddScoped(_ => NewPayrollConsolidatedResultRepository());
            services.AddScoped(_ => NewPayrollResultSetRepository());
        }
        private static IPayrunRepository NewPayrunRepository(bool auditDisabled) =>
            new PayrunRepository(RegulationRepositoryFactory.NewScriptRepository(auditDisabled));

        private static IPayrunParameterRepository NewPayrunParameterRepository() =>
            new PayrunParameterRepository();

        private static IPayrunJobRepository NewPayrunJobRepository() =>
            new PayrunJobRepository(
                NewPayrunJobEmployeeRepository());

        private static IPayrollResultRepository NewPayrollResultRepository() =>
            new PayrollResultRepository();

        private static IPayrollConsolidatedResultRepository NewPayrollConsolidatedResultRepository() =>
            new PayrollConsolidatedResultRepository();

        private static IPayrollResultSetRepository NewPayrollResultSetRepository() =>
            new PayrollResultSetRepository(
                NewWageTypeResultSetRepository(),
                NewCollectorResultSetRepository(),
                NewPayrunResultRepository(),
                true);

        private static IPayrunJobEmployeeRepository NewPayrunJobEmployeeRepository() =>
            new PayrunJobEmployeeRepository();

        private static ICollectorResultRepository NewCollectorResultRepository() =>
            new CollectorResultRepository();

        private static ICollectorResultSetRepository NewCollectorResultSetRepository() =>
            new CollectorResultSetRepository(
                NewCollectorCustomResultRepository(),
                true);

        private static ICollectorCustomResultRepository NewCollectorCustomResultRepository() =>
            new CollectorCustomResultRepository();

        private static IWageTypeResultRepository NewWageTypeResultRepository() =>
            new WageTypeResultRepository();

        private static IWageTypeResultSetRepository NewWageTypeResultSetRepository() =>
            new WageTypeResultSetRepository(
                NewWageTypeCustomResultRepository(),
                true);

        private static IWageTypeCustomResultRepository NewWageTypeCustomResultRepository() =>
            new WageTypeCustomResultRepository();

        private static IPayrunResultRepository NewPayrunResultRepository() =>
            new PayrunResultRepository();
    }

    private static class ReportRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, bool auditDisabled)
        {
            services.AddScoped(_ => NewReportRepository(auditDisabled));
            services.AddScoped(_ => NewReportSetRepository(auditDisabled));
            services.AddScoped(_ => NewReportAuditRepository());
            services.AddScoped(_ => NewReportParameterRepository(auditDisabled));
            services.AddScoped(_ => NewReportParameterAuditRepository());
            services.AddScoped(_ => NewReportTemplateRepository(auditDisabled));
            services.AddScoped(_ => NewReportTemplateAuditRepository());
        }

        private static IReportRepository NewReportRepository(bool auditDisabled) =>
            new ReportRepository(
                RegulationRepositoryFactory.NewRegulationRepository(),
                RegulationRepositoryFactory.NewScriptRepository(auditDisabled),
                NewReportAuditRepository(),
                auditDisabled);

        private static IReportAuditRepository NewReportAuditRepository() =>
            new ReportAuditRepository();

        internal static IReportSetRepository NewReportSetRepository(bool auditDisabled) =>
            new ReportSetRepository(
                new()
                {
                    RegulationRepository = RegulationRepositoryFactory.NewRegulationRepository(),
                    ReportParameterRepository = NewReportParameterRepository(auditDisabled),
                    ReportTemplateRepository = NewReportTemplateRepository(auditDisabled),
                    ScriptRepository = RegulationRepositoryFactory.NewScriptRepository(auditDisabled),
                    AuditRepository = NewReportAuditRepository(),
                    BulkInsert = true
                },
                auditDisabled);

        private static IReportParameterRepository NewReportParameterRepository(bool auditDisabled) =>
            new ReportParameterRepository(
                RegulationRepositoryFactory.NewRegulationRepository(),
                NewReportRepository(auditDisabled),
                new ReportParameterAuditRepository(),
                auditDisabled);

        private static IReportParameterAuditRepository NewReportParameterAuditRepository() =>
            new ReportParameterAuditRepository();

        private static IReportTemplateRepository NewReportTemplateRepository(bool auditDisabled) =>
            new ReportTemplateRepository(
                RegulationRepositoryFactory.NewRegulationRepository(),
                NewReportRepository(auditDisabled),
                new ReportTemplateAuditRepository(),
                auditDisabled);

        private static IReportTemplateAuditRepository NewReportTemplateAuditRepository() =>
            new ReportTemplateAuditRepository();
    }

    #endregion
}