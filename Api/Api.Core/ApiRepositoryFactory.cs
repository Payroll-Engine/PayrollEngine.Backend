using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Persistence;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Api.Core;

internal static class ApiRepositoryFactory
{

    // repositories setup
    internal static void SetupApiServices(IServiceCollection services, IConfiguration configuration)
    {
        // audit trail
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();

        // system repositories
        services.AddScoped(_ => NewRegulationShareRepository());
        TenantRepositoryFactory.SetupRepositories(services);
        RegulationRepositoryFactory.SetupRepositories(services, serverConfiguration.AuditTrail);
        PayrollRepositoryFactory.SetupRepositories(services, serverConfiguration.AuditTrail);
        CaseValueRepositoryFactory.SetupRepositories(services, serverConfiguration.AuditTrail);
        PayrunRepositoryFactory.SetupRepositories(services, serverConfiguration.AuditTrail);
        ReportRepositoryFactory.SetupRepositories(services, serverConfiguration.AuditTrail);
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
        internal static void SetupRepositories(IServiceCollection services, AuditTrailConfiguration auditTrail)
        {
            // regulation repository
            services.AddScoped(_ => NewRegulationRepository());
            // case repositories
            services.AddScoped(_ => NewCaseRepository(auditTrail.Input));
            services.AddScoped(_ => NewCaseAuditRepository());
            services.AddScoped(_ => NewCaseFieldRepository(auditTrail.Input));
            services.AddScoped(_ => NewCaseFieldAuditRepository());
            services.AddScoped(_ => NewCaseRelationRepository(auditTrail.Input));
            services.AddScoped(_ => NewCaseRelationAuditService());
            // wage type repositories
            services.AddScoped(_ => NewWageTypeRepository(auditTrail.Payrun));
            services.AddScoped(_ => NewWageTypeAuditRepository());
            // collector repositories
            services.AddScoped(_ => NewCollectorRepository(auditTrail.Payrun));
            services.AddScoped(_ => NewCollectorAuditRepository());
            // lookup repositories
            services.AddScoped(_ => NewLookupRepository(auditTrail.Lookup));
            services.AddScoped(_ => NewLookupAuditRepository());
            services.AddScoped(_ => NewLookupValueRepository(auditTrail.Lookup));
            services.AddScoped(_ => NewLookupValueAuditRepository());
            services.AddScoped(_ => NewLookupSetRepository(auditTrail.Lookup));
            // script repositories
            services.AddScoped(_ => NewScriptRepository(auditTrail.Script));
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
        internal static void SetupRepositories(IServiceCollection services, AuditTrailConfiguration auditTrail)
        {
            services.AddScoped(_ => NewPayrollRepository(auditTrail));
            services.AddScoped(_ => NewPayrollLayerRepository());
        }

        internal static IPayrollRepository NewPayrollRepository(AuditTrailConfiguration auditTrail) =>
            new PayrollRepository(
                NewPayrollLayerRepository(),
                RegulationRepositoryFactory.NewRegulationRepository(),
                RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                ReportRepositoryFactory.NewReportSetRepository(auditTrail.Report),
                RegulationRepositoryFactory.NewScriptRepository(auditTrail.Script));

        private static IPayrollLayerRepository NewPayrollLayerRepository() =>
            new PayrollLayerRepository();
    }

    private static class CaseValueRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, AuditTrailConfiguration auditTrail)
        {
            // global case repositories
            services.AddScoped(_ => NewGlobalCaseValueRepository(auditTrail.Input));
            services.AddScoped(_ => NewGlobalCaseDocumentRepository());
            services.AddScoped(_ => NewGlobalCaseChangeRepository(auditTrail));
            // national case repositories
            services.AddScoped(_ => NewNationalCaseValueRepository(auditTrail.Input));
            services.AddScoped(_ => NewNationalCaseDocumentRepository());
            services.AddScoped(_ => NewNationalCaseChangeRepository(auditTrail));
            // company case repositories
            services.AddScoped(_ => NewCompanyCaseValueRepository(auditTrail.Input));
            services.AddScoped(_ => NewCompanyCaseDocumentRepository());
            services.AddScoped(_ => NewCompanyCaseChangeRepository(auditTrail));
            // employee case repositories
            services.AddScoped(_ => NewEmployeeCaseValueRepository(auditTrail.Input));
            services.AddScoped(_ => NewEmployeeCaseDocumentRepository());
            services.AddScoped(_ => NewEmployeeCaseChangeRepository(auditTrail));
        }

        private static IGlobalCaseValueRepository NewGlobalCaseValueRepository(bool auditDisabled) =>
            new GlobalCaseValueRepository(RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static IGlobalCaseDocumentRepository NewGlobalCaseDocumentRepository() =>
            new GlobalCaseDocumentRepository();

        private static IGlobalCaseChangeRepository NewGlobalCaseChangeRepository(AuditTrailConfiguration auditTrail) =>
            new GlobalCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditTrail),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditTrail.Input),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                    CaseValueRepository = NewGlobalCaseValueRepository(auditTrail.Input),
                    CaseValueSetupRepository = new GlobalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                        new GlobalCaseDocumentRepository()),
                    CaseValueChangeRepository = new GlobalCaseValueChangeRepository()
                });

        private static INationalCaseValueRepository NewNationalCaseValueRepository(bool auditDisabled) =>
            new NationalCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static INationalCaseDocumentRepository NewNationalCaseDocumentRepository() =>
            new NationalCaseDocumentRepository();

        private static INationalCaseChangeRepository NewNationalCaseChangeRepository(AuditTrailConfiguration auditTrail) =>
            new NationalCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditTrail),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditTrail.Input),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                    CaseValueRepository = NewNationalCaseValueRepository(auditTrail.Input),
                    CaseValueSetupRepository = new NationalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                        new NationalCaseDocumentRepository()),
                    CaseValueChangeRepository = new NationalCaseValueChangeRepository()
                });

        private static ICompanyCaseValueRepository NewCompanyCaseValueRepository(bool auditDisabled) =>
            new CompanyCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static ICompanyCaseDocumentRepository NewCompanyCaseDocumentRepository() =>
            new CompanyCaseDocumentRepository();

        private static ICompanyCaseChangeRepository NewCompanyCaseChangeRepository(AuditTrailConfiguration auditTrail) =>
            new CompanyCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditTrail),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditTrail.Input),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                    CaseValueRepository = NewCompanyCaseValueRepository(auditTrail.Input),
                    CaseValueSetupRepository = new CompanyCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                        new CompanyCaseDocumentRepository()),
                    CaseValueChangeRepository = new CompanyCaseValueChangeRepository()
                });

        private static IEmployeeCaseValueRepository NewEmployeeCaseValueRepository(bool auditDisabled) =>
            new EmployeeCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(auditDisabled));

        private static IEmployeeCaseDocumentRepository NewEmployeeCaseDocumentRepository() =>
            new EmployeeCaseDocumentRepository();

        private static IEmployeeCaseChangeRepository NewEmployeeCaseChangeRepository(AuditTrailConfiguration auditTrail) =>
            new EmployeeCaseChangeRepository(
                new()
                {
                    TenantRepository = new TenantRepository(),
                    DivisionRepository = new DivisionRepository(),
                    EmployeeRepository = new EmployeeRepository(
                        new EmployeeDivisionRepository(
                            new DivisionRepository())),
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(auditTrail),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(auditTrail.Input),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                    CaseValueRepository = NewEmployeeCaseValueRepository(auditTrail.Input),
                    CaseValueSetupRepository = new EmployeeCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(auditTrail.Input),
                        NewEmployeeCaseDocumentRepository()),
                    CaseValueChangeRepository = new EmployeeCaseValueChangeRepository()
                });
    }

    private static class PayrunRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, AuditTrailConfiguration auditTrail)
        {
            // payrun repositories
            services.AddScoped(_ => NewPayrunRepository(auditTrail));
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
        private static IPayrunRepository NewPayrunRepository(AuditTrailConfiguration auditTrail) =>
            new PayrunRepository(RegulationRepositoryFactory.NewScriptRepository(auditTrail.Script));

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
        internal static void SetupRepositories(IServiceCollection services, AuditTrailConfiguration auditTrail)
        {
            services.AddScoped(_ => NewReportRepository(auditTrail.Report));
            services.AddScoped(_ => NewReportSetRepository(auditTrail.Report));
            services.AddScoped(_ => NewReportAuditRepository());
            services.AddScoped(_ => NewReportParameterRepository(auditTrail.Report));
            services.AddScoped(_ => NewReportParameterAuditRepository());
            services.AddScoped(_ => NewReportTemplateRepository(auditTrail.Report));
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