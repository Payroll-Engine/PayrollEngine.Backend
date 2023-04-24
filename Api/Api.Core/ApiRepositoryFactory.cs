using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;
using PayrollEngine.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace PayrollEngine.Api.Core;

internal static class ApiRepositoryFactory
{

    // repositories setup
    internal static void SetupApiServices(IServiceCollection services)
    {
        // system repositories
        services.AddScoped(_ => NewRegulationPermissionRepository());
        TenantRepositoryFactory.SetupRepositories(services);
        RegulationRepositoryFactory.SetupRepositories(services);
        PayrollRepositoryFactory.SetupRepositories(services);
        CaseValueRepositoryFactory.SetupRepositories(services);
        PayrunRepositoryFactory.SetupRepositories(services);
        ReportRepositoryFactory.SetupRepositories(services);
    }

    private static IRegulationPermissionRepository NewRegulationPermissionRepository() =>
        new RegulationPermissionRepository(
            RegulationRepositoryFactory.NewRegulationRepository());

    #region Repository Factories

    private static class TenantRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services)
        {
            services.AddScoped(_ => NewTenantRepository());
            services.AddScoped(_ => NewWebhookRepository());
            services.AddScoped(_ => NewWebhookMessageRepository());
            services.AddScoped(_ => NewUserRepository());
            services.AddScoped(_ => NewDivisionRepository());
            services.AddScoped(_ => NewTaskRepository());
            services.AddScoped(_ => NewLogRepository());
            services.AddScoped(_ => NewReportLogRepository());
            services.AddScoped(_ => NewEmployeeDivisionRepository());
            services.AddScoped(_ => NewEmployeeRepository());
        }

        private static ITenantRepository NewTenantRepository() =>
            new TenantRepository();

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

        internal static IEmployeeRepository NewEmployeeRepository() =>
            new EmployeeRepository(
                NewEmployeeDivisionRepository());
    }

    private static class RegulationRepositoryFactory
    {
        internal static void SetupRepositories(IServiceCollection services)
        {
            // regulation repository
            services.AddScoped(_ => NewRegulationRepository());
            // case repositories
            services.AddScoped(_ => NewCaseRepository());
            services.AddScoped(_ => NewCaseAuditRepository());
            services.AddScoped(_ => NewCaseFieldRepository());
            services.AddScoped(_ => NewCaseFieldAuditRepository());
            services.AddScoped(_ => NewCaseRelationRepository());
            services.AddScoped(_ => NewCaseRelationAuditService());
            // wage type repositories
            services.AddScoped(_ => NewWageTypeRepository());
            services.AddScoped(_ => NewWageTypeAuditRepository());
            // collector repositories
            services.AddScoped(_ => NewCollectorRepository());
            services.AddScoped(_ => NewCollectorAuditRepository());
            // lookup repositories
            services.AddScoped(_ => NewLookupRepository());
            services.AddScoped(_ => NewLookupAuditRepository());
            services.AddScoped(_ => NewLookupValueRepository());
            services.AddScoped(_ => NewLookupValueAuditRepository());
            services.AddScoped(_ => NewLookupSetRepository());
            // script repositories
            services.AddScoped(_ => NewScriptRepository());
            services.AddScoped(_ => NewScriptAuditRepository());
            services.AddScoped(_ => NewScriptProvider());
        }

        internal static IRegulationRepository NewRegulationRepository() =>
            new RegulationRepository();

        private static ICaseAuditRepository NewCaseAuditRepository() =>
            new CaseAuditRepository();

        internal static ICaseFieldRepository NewCaseFieldRepository() =>
            new CaseFieldRepository(
                NewCaseRepository(),
                NewCaseFieldAuditRepository());

        private static ICaseFieldAuditRepository NewCaseFieldAuditRepository() =>
            new CaseFieldAuditRepository();

        private static ICaseRelationRepository NewCaseRelationRepository() =>
            new CaseRelationRepository(
                new CaseRelationScriptController(),
                NewScriptRepository(),
                NewCaseRelationAuditService());

        private static ICaseRelationAuditRepository NewCaseRelationAuditService() =>
            new CaseRelationAuditRepository();

        private static IWageTypeRepository NewWageTypeRepository() =>
            new WageTypeRepository(
                new WageTypeScriptController(),
                NewScriptRepository(),
                NewWageTypeAuditRepository());

        private static IWageTypeAuditRepository NewWageTypeAuditRepository() =>
            new WageTypeAuditRepository();

        internal static ICaseRepository NewCaseRepository() =>
            new CaseRepository(
                new CaseScriptController(),
                NewScriptRepository(),
                NewCaseAuditRepository());

        private static ICollectorAuditRepository NewCollectorAuditRepository() =>
            new CollectorAuditRepository();

        private static ICollectorRepository NewCollectorRepository() =>
            new CollectorRepository(
                new CollectorScriptController(),
                NewScriptRepository(),
                NewCollectorAuditRepository());

        private static ILookupValueAuditRepository NewLookupValueAuditRepository() =>
            new LookupValueAuditRepository();

        private static ILookupValueRepository NewLookupValueRepository() =>
            new LookupValueRepository(
                NewLookupValueAuditRepository());

        private static ILookupAuditRepository NewLookupAuditRepository() =>
            new LookupAuditRepository();

        private static ILookupRepository NewLookupRepository() =>
            new LookupRepository(
                NewLookupAuditRepository());

        private static ILookupSetRepository NewLookupSetRepository() =>
            new LookupSetRepository(
                NewLookupValueRepository(),
                NewLookupAuditRepository());

        private static IScriptAuditRepository NewScriptAuditRepository() =>
            new ScriptAuditRepository();

        internal static IScriptRepository NewScriptRepository() =>
            new ScriptRepository(
                NewScriptAuditRepository());

        private static IScriptProvider NewScriptProvider() =>
            new ScriptProviderRepository();
    }

    private static class PayrollRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services)
        {
            services.AddScoped(_ => NewPayrollRepository());
            services.AddScoped(_ => NewPayrollLayerRepository());
        }

        internal static IPayrollRepository NewPayrollRepository() =>
            new PayrollRepository(
                NewPayrollLayerRepository(),
                RegulationRepositoryFactory.NewRegulationRepository(),
                RegulationRepositoryFactory.NewCaseFieldRepository(),
                ReportRepositoryFactory.NewReportSetRepository(),
                RegulationRepositoryFactory.NewScriptRepository());

        private static IPayrollLayerRepository NewPayrollLayerRepository() =>
            new PayrollLayerRepository();
    }

    private static class CaseValueRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services)
        {
            // global case repositories
            services.AddScoped(_ => NewGlobalCaseValueRepository());
            services.AddScoped(_ => NewGlobalCaseDocumentRepository());
            services.AddScoped(_ => NewGlobalCaseChangeRepository());
            // national case repositories
            services.AddScoped(_ => NewNationalCaseValueRepository());
            services.AddScoped(_ => NewNationalCaseDocumentRepository());
            services.AddScoped(_ => NewNationalCaseChangeRepository());
            // company case repositories
            services.AddScoped(_ => NewCompanyCaseValueRepository());
            services.AddScoped(_ => NewCompanyCaseDocumentRepository());
            services.AddScoped(_ => NewCompanyCaseChangeRepository());
            // employee case repositories
            services.AddScoped(_ => NewEmployeeCaseValueRepository());
            services.AddScoped(_ => NewEmployeeCaseDocumentRepository());
            services.AddScoped(_ => NewEmployeeCaseChangeRepository());
        }

        private static IGlobalCaseValueRepository NewGlobalCaseValueRepository() =>
            new GlobalCaseValueRepository(RegulationRepositoryFactory.NewCaseFieldRepository());

        private static IGlobalCaseDocumentRepository NewGlobalCaseDocumentRepository() =>
            new GlobalCaseDocumentRepository();

        private static IGlobalCaseChangeRepository NewGlobalCaseChangeRepository() =>
            new GlobalCaseChangeRepository(
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(),
                    CaseValueRepository = NewGlobalCaseValueRepository(),
                    CaseValueSetupRepository = new GlobalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(),
                        new GlobalCaseDocumentRepository()),
                    CaseValueChangeRepository = new GlobalCaseValueChangeRepository()
                });

        private static INationalCaseValueRepository NewNationalCaseValueRepository() =>
            new NationalCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository());

        private static INationalCaseDocumentRepository NewNationalCaseDocumentRepository() =>
            new NationalCaseDocumentRepository();

        private static INationalCaseChangeRepository NewNationalCaseChangeRepository() =>
            new NationalCaseChangeRepository(
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(),
                    CaseValueRepository = NewNationalCaseValueRepository(),
                    CaseValueSetupRepository = new NationalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(),
                        new NationalCaseDocumentRepository()),
                    CaseValueChangeRepository = new NationalCaseValueChangeRepository()
                });

        private static ICompanyCaseValueRepository NewCompanyCaseValueRepository() =>
            new CompanyCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository());

        private static ICompanyCaseDocumentRepository NewCompanyCaseDocumentRepository() =>
            new CompanyCaseDocumentRepository();

        private static ICompanyCaseChangeRepository NewCompanyCaseChangeRepository() =>
            new CompanyCaseChangeRepository(
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(),
                    CaseValueRepository = NewCompanyCaseValueRepository(),
                    CaseValueSetupRepository = new CompanyCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(),
                        new CompanyCaseDocumentRepository()),
                    CaseValueChangeRepository = new CompanyCaseValueChangeRepository()
                });

        private static IEmployeeCaseValueRepository NewEmployeeCaseValueRepository() =>
            new EmployeeCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository());

        private static IEmployeeCaseDocumentRepository NewEmployeeCaseDocumentRepository() =>
            new EmployeeCaseDocumentRepository();

        private static IEmployeeCaseChangeRepository NewEmployeeCaseChangeRepository() =>
            new EmployeeCaseChangeRepository(
                TenantRepositoryFactory.NewEmployeeRepository(),
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(),
                    CaseValueRepository = NewEmployeeCaseValueRepository(),
                    CaseValueSetupRepository = new EmployeeCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(),
                        NewEmployeeCaseDocumentRepository()),
                    CaseValueChangeRepository = new EmployeeCaseValueChangeRepository()
                });
    }

    private static class PayrunRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services)
        {
            // payrun repositories
            services.AddScoped(_ => NewPayrunRepository());
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
        private static IPayrunRepository NewPayrunRepository() =>
            new PayrunRepository(new PayrunScriptController(),
                RegulationRepositoryFactory.NewScriptRepository());

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
        internal static void SetupRepositories(IServiceCollection services)
        {
            services.AddScoped(_ => NewReportRepository());
            services.AddScoped(_ => NewReportSetRepository());
            services.AddScoped(_ => NewReportAuditRepository());
            services.AddScoped(_ => NewReportParameterRepository());
            services.AddScoped(_ => NewReportParameterAuditRepository());
            services.AddScoped(_ => NewReportTemplateRepository());
            services.AddScoped(_ => NewReportTemplateAuditRepository());
        }

        private static IReportRepository NewReportRepository() =>
            new ReportRepository(
                new ReportScriptController<ReportSet>(),
                RegulationRepositoryFactory.NewScriptRepository(),
                NewReportAuditRepository());

        private static IReportAuditRepository NewReportAuditRepository() =>
            new ReportAuditRepository();

        internal static IReportSetRepository NewReportSetRepository() =>
            new ReportSetRepository(
                new()
                {
                    ReportParameterRepository = NewReportParameterRepository(),
                    ReportTemplateRepository = NewReportTemplateRepository(),
                    ScriptController = new ReportScriptController<ReportSet>(),
                    ScriptRepository = RegulationRepositoryFactory.NewScriptRepository(),
                    AuditRepository = NewReportAuditRepository(),
                    BulkInsert = true
                });

        private static IReportParameterRepository NewReportParameterRepository() =>
            new ReportParameterRepository(
                new ReportParameterAuditRepository());

        private static IReportParameterAuditRepository NewReportParameterAuditRepository() =>
            new ReportParameterAuditRepository();

        private static IReportTemplateRepository NewReportTemplateRepository() =>
            new ReportTemplateRepository(
                new ReportTemplateAuditRepository());

        private static IReportTemplateAuditRepository NewReportTemplateAuditRepository() =>
            new ReportTemplateAuditRepository();
    }

    #endregion
}