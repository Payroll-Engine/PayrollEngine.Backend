using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;
using PayrollEngine.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace PayrollEngine.Api.Core;

internal static class ApiRepositoryFactory
{

    // repositories setup
    internal static void SetupApiServices(IServiceCollection services, IDbContext context)
    {
        // system repositories
        services.AddScoped(_ => NewRegulationPermissionRepository(context));
        TenantRepositoryFactory.SetupRepositories(services, context);
        RegulationRepositoryFactory.SetupRepositories(services, context);
        PayrollRepositoryFactory.SetupRepositories(services, context);
        CaseValueRepositoryFactory.SetupRepositories(services, context);
        PayrunRepositoryFactory.SetupRepositories(services, context);
        ReportRepositoryFactory.SetupRepositories(services, context);
    }

    private static IRegulationPermissionRepository NewRegulationPermissionRepository(IDbContext context) =>
        new RegulationPermissionRepository(
            RegulationRepositoryFactory.NewRegulationRepository(context),
            context);

    #region Repository Factories

    private static class TenantRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, IDbContext context)
        {
            services.AddScoped(_ => NewTenantRepository(context));
            services.AddScoped(_ => NewWebhookRepository(context));
            services.AddScoped(_ => NewWebhookMessageRepository(context));
            services.AddScoped(_ => NewUserRepository(context));
            services.AddScoped(_ => NewDivisionRepository(context));
            services.AddScoped(_ => NewTaskRepository(context));
            services.AddScoped(_ => NewLogRepository(context));
            services.AddScoped(_ => NewReportLogRepository(context));
            services.AddScoped(_ => NewEmployeeDivisionRepository(context));
            services.AddScoped(_ => NewEmployeeRepository(context));
        }

        private static ITenantRepository NewTenantRepository(IDbContext context) =>
            new TenantRepository(context);

        private static IWebhookRepository NewWebhookRepository(IDbContext context) =>
            new WebhookRepository(context);

        private static IWebhookMessageRepository NewWebhookMessageRepository(IDbContext context) =>
            new WebhookMessageRepository(context);

        private static IUserRepository NewUserRepository(IDbContext context) =>
            new UserRepository(context);

        private static IDivisionRepository NewDivisionRepository(IDbContext context) =>
            new DivisionRepository(context);

        private static ITaskRepository NewTaskRepository(IDbContext context) =>
            new TaskRepository(context);

        private static ILogRepository NewLogRepository(IDbContext context) =>
            new LogRepository(context);

        private static IReportLogRepository NewReportLogRepository(IDbContext context) =>
            new ReportLogRepository(context);

        private static IEmployeeDivisionRepository NewEmployeeDivisionRepository(IDbContext context) =>
            new EmployeeDivisionRepository(
                NewDivisionRepository(context),
                context);

        internal static IEmployeeRepository NewEmployeeRepository(IDbContext context) =>
            new EmployeeRepository(
                NewEmployeeDivisionRepository(context),
                context);
    }

    private static class RegulationRepositoryFactory
    {
        internal static void SetupRepositories(IServiceCollection services, IDbContext context)
        {
            // regulation repository
            services.AddScoped(_ => NewRegulationRepository(context));
            // case repositories
            services.AddScoped(_ => NewCaseRepository(context));
            services.AddScoped(_ => NewCaseAuditRepository(context));
            services.AddScoped(_ => NewCaseFieldRepository(context));
            services.AddScoped(_ => NewCaseFieldAuditRepository(context));
            services.AddScoped(_ => NewCaseRelationRepository(context));
            services.AddScoped(_ => NewCaseRelationAuditService(context));
            // wage type repositories
            services.AddScoped(_ => NewWageTypeRepository(context));
            services.AddScoped(_ => NewWageTypeAuditRepository(context));
            // collector repositories
            services.AddScoped(_ => NewCollectorRepository(context));
            services.AddScoped(_ => NewCollectorAuditRepository(context));
            // lookup repositories
            services.AddScoped(_ => NewLookupRepository(context));
            services.AddScoped(_ => NewLookupAuditRepository(context));
            services.AddScoped(_ => NewLookupValueRepository(context));
            services.AddScoped(_ => NewLookupValueAuditRepository(context));
            services.AddScoped(_ => NewLookupSetRepository(context));
            // script repositories
            services.AddScoped(_ => NewScriptRepository(context));
            services.AddScoped(_ => NewScriptAuditRepository(context));
            services.AddScoped(_ => NewScriptProvider(context));
        }

        internal static IRegulationRepository NewRegulationRepository(IDbContext context) =>
            new RegulationRepository(
                context);

        private static ICaseAuditRepository NewCaseAuditRepository(IDbContext context) =>
            new CaseAuditRepository(context);

        internal static ICaseFieldRepository NewCaseFieldRepository(IDbContext context) =>
            new CaseFieldRepository(
                NewCaseRepository(context),
                NewCaseFieldAuditRepository(context),
                context);

        private static ICaseFieldAuditRepository NewCaseFieldAuditRepository(IDbContext context) =>
            new CaseFieldAuditRepository(context);

        private static ICaseRelationRepository NewCaseRelationRepository(IDbContext context) =>
            new CaseRelationRepository(
                new CaseRelationScriptController(),
                NewScriptRepository(context),
                NewCaseRelationAuditService(context),
                context);

        private static ICaseRelationAuditRepository NewCaseRelationAuditService(IDbContext context) =>
            new CaseRelationAuditRepository(context);

        private static IWageTypeRepository NewWageTypeRepository(IDbContext context) =>
            new WageTypeRepository(
                new WageTypeScriptController(),
                NewScriptRepository(context),
                NewWageTypeAuditRepository(context),
                context);

        private static IWageTypeAuditRepository NewWageTypeAuditRepository(IDbContext context) =>
            new WageTypeAuditRepository(context);

        internal static ICaseRepository NewCaseRepository(IDbContext context) =>
            new CaseRepository(
                new CaseScriptController(),
                NewScriptRepository(context),
                NewCaseAuditRepository(context),
                context);

        private static ICollectorAuditRepository NewCollectorAuditRepository(IDbContext context) =>
            new CollectorAuditRepository(context);

        private static ICollectorRepository NewCollectorRepository(IDbContext context) =>
            new CollectorRepository(
                new CollectorScriptController(),
                NewScriptRepository(context),
                NewCollectorAuditRepository(context),
                context);

        private static ILookupValueAuditRepository NewLookupValueAuditRepository(IDbContext context) =>
            new LookupValueAuditRepository(context);

        private static ILookupValueRepository NewLookupValueRepository(IDbContext context) =>
            new LookupValueRepository(
                NewLookupValueAuditRepository(context),
                context);

        private static ILookupAuditRepository NewLookupAuditRepository(IDbContext context) =>
            new LookupAuditRepository(context);

        private static ILookupRepository NewLookupRepository(IDbContext context) =>
            new LookupRepository(
                NewLookupAuditRepository(context),
                context);

        private static ILookupSetRepository NewLookupSetRepository(IDbContext context) =>
            new LookupSetRepository(
                NewLookupValueRepository(context),
                NewLookupAuditRepository(context),
                context);

        private static IScriptAuditRepository NewScriptAuditRepository(IDbContext context) =>
            new ScriptAuditRepository(context);

        internal static IScriptRepository NewScriptRepository(IDbContext context) =>
            new ScriptRepository(
                NewScriptAuditRepository(context),
                context);

        private static IScriptProvider NewScriptProvider(IDbContext context) =>
            new ScriptProviderRepository(
                context);
    }

    private static class PayrollRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, IDbContext context)
        {
            services.AddScoped(_ => NewPayrollRepository(context));
            services.AddScoped(_ => NewPayrollLayerRepository(context));
        }

        internal static IPayrollRepository NewPayrollRepository(IDbContext context) =>
            new PayrollRepository(
                NewPayrollLayerRepository(context),
                RegulationRepositoryFactory.NewRegulationRepository(context),
                RegulationRepositoryFactory.NewCaseFieldRepository(context),
                ReportRepositoryFactory.NewReportSetRepository(context),
                RegulationRepositoryFactory.NewScriptRepository(context),
                context);

        private static IPayrollLayerRepository NewPayrollLayerRepository(IDbContext context) =>
            new PayrollLayerRepository(context);
    }

    private static class CaseValueRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, IDbContext context)
        {
            // global case repositories
            services.AddScoped(_ => NewGlobalCaseValueRepository(context));
            services.AddScoped(_ => NewGlobalCaseDocumentRepository(context));
            services.AddScoped(_ => NewGlobalCaseChangeRepository(context));
            // national case repositories
            services.AddScoped(_ => NewNationalCaseValueRepository(context));
            services.AddScoped(_ => NewNationalCaseDocumentRepository(context));
            services.AddScoped(_ => NewNationalCaseChangeRepository(context));
            // company case repositories
            services.AddScoped(_ => NewCompanyCaseValueRepository(context));
            services.AddScoped(_ => NewCompanyCaseDocumentRepository(context));
            services.AddScoped(_ => NewCompanyCaseChangeRepository(context));
            // employee case repositories
            services.AddScoped(_ => NewEmployeeCaseValueRepository(context));
            services.AddScoped(_ => NewEmployeeCaseDocumentRepository(context));
            services.AddScoped(_ => NewEmployeeCaseChangeRepository(context));
        }

        private static IGlobalCaseValueRepository NewGlobalCaseValueRepository(IDbContext context) =>
            new GlobalCaseValueRepository(RegulationRepositoryFactory.NewCaseFieldRepository(context),
                context);

        private static IGlobalCaseDocumentRepository NewGlobalCaseDocumentRepository(IDbContext context) =>
            new GlobalCaseDocumentRepository(context);

        private static IGlobalCaseChangeRepository NewGlobalCaseChangeRepository(IDbContext context) =>
            new GlobalCaseChangeRepository(
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(context),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(context),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(context),
                    CaseValueRepository = NewGlobalCaseValueRepository(context),
                    CaseValueSetupRepository = new GlobalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(context),
                        new GlobalCaseDocumentRepository(context),
                        context),
                    CaseValueChangeRepository = new GlobalCaseValueChangeRepository(context)
                },
                context);

        private static INationalCaseValueRepository NewNationalCaseValueRepository(IDbContext context) =>
            new NationalCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(context),
                context);

        private static INationalCaseDocumentRepository NewNationalCaseDocumentRepository(IDbContext context) =>
            new NationalCaseDocumentRepository(context);

        private static INationalCaseChangeRepository NewNationalCaseChangeRepository(IDbContext context) =>
            new NationalCaseChangeRepository(
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(context),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(context),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(context),
                    CaseValueRepository = NewNationalCaseValueRepository(context),
                    CaseValueSetupRepository = new NationalCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(context),
                        new NationalCaseDocumentRepository(context),
                        context),
                    CaseValueChangeRepository = new NationalCaseValueChangeRepository(context)
                },
                context);

        private static ICompanyCaseValueRepository NewCompanyCaseValueRepository(IDbContext context) =>
            new CompanyCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(context),
                context);

        private static ICompanyCaseDocumentRepository NewCompanyCaseDocumentRepository(IDbContext context) =>
            new CompanyCaseDocumentRepository(context);

        private static ICompanyCaseChangeRepository NewCompanyCaseChangeRepository(IDbContext context) =>
            new CompanyCaseChangeRepository(
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(context),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(context),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(context),
                    CaseValueRepository = NewCompanyCaseValueRepository(context),
                    CaseValueSetupRepository = new CompanyCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(context),
                        new CompanyCaseDocumentRepository(context),
                        context),
                    CaseValueChangeRepository = new CompanyCaseValueChangeRepository(context)
                },
                context);

        private static IEmployeeCaseValueRepository NewEmployeeCaseValueRepository(IDbContext context) =>
            new EmployeeCaseValueRepository(
                RegulationRepositoryFactory.NewCaseFieldRepository(context),
                context);

        private static IEmployeeCaseDocumentRepository NewEmployeeCaseDocumentRepository(IDbContext context) =>
            new EmployeeCaseDocumentRepository(context);

        private static IEmployeeCaseChangeRepository NewEmployeeCaseChangeRepository(IDbContext context) =>
            new EmployeeCaseChangeRepository(
                TenantRepositoryFactory.NewEmployeeRepository(context),
                new()
                {
                    PayrollRepository = PayrollRepositoryFactory.NewPayrollRepository(context),
                    CaseRepository = RegulationRepositoryFactory.NewCaseRepository(context),
                    CaseFieldRepository = RegulationRepositoryFactory.NewCaseFieldRepository(context),
                    CaseValueRepository = NewEmployeeCaseValueRepository(context),
                    CaseValueSetupRepository = new EmployeeCaseValueSetupRepository(
                        RegulationRepositoryFactory.NewCaseFieldRepository(context),
                        NewEmployeeCaseDocumentRepository(context),
                        context),
                    CaseValueChangeRepository = new EmployeeCaseValueChangeRepository(context)
                },
                context);
    }

    private static class PayrunRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, IDbContext context)
        {
            // payrun repositories
            services.AddScoped(_ => NewPayrunRepository(context));
            services.AddScoped(_ => NewPayrunParameterRepository(context));
            services.AddScoped(_ => NewPayrunJobRepository(context));
            // collector results repositories
            services.AddScoped(_ => NewCollectorResultRepository(context));
            services.AddScoped(_ => NewCollectorResultSetRepository(context));
            services.AddScoped(_ => NewCollectorCustomResultRepository(context));
            // wage type results repositories
            services.AddScoped(_ => NewWageTypeResultRepository(context));
            services.AddScoped(_ => NewWageTypeResultSetRepository(context));
            services.AddScoped(_ => NewWageTypeCustomResultRepository(context));
            // payrun results repositories
            services.AddScoped(_ => NewPayrunResultRepository(context));
            // payroll results repositories
            services.AddScoped(_ => NewPayrollResultRepository(context));
            services.AddScoped(_ => NewPayrollConsolidatedResultRepository(context));
            services.AddScoped(_ => NewPayrollResultSetRepository(context));
        }
        private static IPayrunRepository NewPayrunRepository(IDbContext context) =>
            new PayrunRepository(new PayrunScriptController(),
                RegulationRepositoryFactory.NewScriptRepository(context),
                context);

        private static IPayrunParameterRepository NewPayrunParameterRepository(IDbContext context) =>
            new PayrunParameterRepository(
                context);

        private static IPayrunJobRepository NewPayrunJobRepository(IDbContext context) =>
            new PayrunJobRepository(
                NewPayrunJobEmployeeRepository(context),
                context);

        private static IPayrollResultRepository NewPayrollResultRepository(IDbContext context) =>
            new PayrollResultRepository(context);

        private static IPayrollConsolidatedResultRepository NewPayrollConsolidatedResultRepository(IDbContext context) =>
            new PayrollConsolidatedResultRepository(context);

        private static IPayrollResultSetRepository NewPayrollResultSetRepository(IDbContext context) =>
            new PayrollResultSetRepository(
                NewWageTypeResultSetRepository(context),
                NewCollectorResultSetRepository(context),
                NewPayrunResultRepository(context),
                true, context);

        private static IPayrunJobEmployeeRepository NewPayrunJobEmployeeRepository(IDbContext context) =>
            new PayrunJobEmployeeRepository(context);

        private static ICollectorResultRepository NewCollectorResultRepository(IDbContext context) =>
            new CollectorResultRepository(
                context);

        private static ICollectorResultSetRepository NewCollectorResultSetRepository(IDbContext context) =>
            new CollectorResultSetRepository(
                NewCollectorCustomResultRepository(context),
                true, context);

        private static ICollectorCustomResultRepository NewCollectorCustomResultRepository(IDbContext context) =>
            new CollectorCustomResultRepository(
                context);

        private static IWageTypeResultRepository NewWageTypeResultRepository(IDbContext context) =>
            new WageTypeResultRepository(
                context);

        private static IWageTypeResultSetRepository NewWageTypeResultSetRepository(IDbContext context) =>
            new WageTypeResultSetRepository(
                NewWageTypeCustomResultRepository(context),
                true, context);

        private static IWageTypeCustomResultRepository NewWageTypeCustomResultRepository(IDbContext context) =>
            new WageTypeCustomResultRepository(
                context);

        private static IPayrunResultRepository NewPayrunResultRepository(IDbContext context) =>
            new PayrunResultRepository(
                context);
    }

    private static class ReportRepositoryFactory
    {
        // repositories setup
        internal static void SetupRepositories(IServiceCollection services, IDbContext context)
        {
            services.AddScoped(_ => NewReportRepository(context));
            services.AddScoped(_ => NewReportSetRepository(context));
            services.AddScoped(_ => NewReportAuditRepository(context));
            services.AddScoped(_ => NewReportParameterRepository(context));
            services.AddScoped(_ => NewReportParameterAuditRepository(context));
            services.AddScoped(_ => NewReportTemplateRepository(context));
            services.AddScoped(_ => NewReportTemplateAuditRepository(context));
        }

        private static IReportRepository NewReportRepository(IDbContext context) =>
            new ReportRepository(
                new ReportScriptController<ReportSet>(),
                RegulationRepositoryFactory.NewScriptRepository(context),
                NewReportAuditRepository(context),
                context);

        private static IReportAuditRepository NewReportAuditRepository(IDbContext context) =>
            new ReportAuditRepository(
                context);

        internal static IReportSetRepository NewReportSetRepository(IDbContext context) =>
            new ReportSetRepository(
                new()
                {
                    ReportParameterRepository = NewReportParameterRepository(context),
                    ReportTemplateRepository = NewReportTemplateRepository(context),
                    ScriptController = new ReportScriptController<ReportSet>(),
                    ScriptRepository = RegulationRepositoryFactory.NewScriptRepository(context),
                    AuditRepository = NewReportAuditRepository(context),
                    BulkInsert = true
                },
                context);

        private static IReportParameterRepository NewReportParameterRepository(IDbContext context) =>
            new ReportParameterRepository(
                new ReportParameterAuditRepository(context),
                context);

        private static IReportParameterAuditRepository NewReportParameterAuditRepository(IDbContext context) =>
            new ReportParameterAuditRepository(
                context);

        private static IReportTemplateRepository NewReportTemplateRepository(IDbContext context) =>
            new ReportTemplateRepository(
                new ReportTemplateAuditRepository(context),
                context);

        private static IReportTemplateAuditRepository NewReportTemplateAuditRepository(IDbContext context) =>
            new ReportTemplateAuditRepository(
                context);
    }

    #endregion
}