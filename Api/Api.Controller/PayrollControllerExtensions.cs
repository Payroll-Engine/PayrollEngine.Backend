using System;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Api.Controller;

internal static class PayrollControllerExtensions
{
    extension(PayrollController controller)
    {
        internal DerivedCaseValidator NewCaseValidator(CaseSetupSettings setup)
        {
            // server configuration
            var serverConfiguration = controller.Configuration.GetConfiguration<PayrollServerConfiguration>();

            var settings = new DerivedCaseToolSettings
            {
                DbContext = setup.DbContext,
                Tenant = setup.Tenant,
                Culture = setup.Culture,
                Calendar = setup.Calendar,
                User = setup.User,
                Payroll = setup.Payroll,
                TaskRepository = controller.TaskService.Repository,
                LogRepository = controller.LogService.Repository,
                DivisionRepository = controller.DivisionService.Repository,
                EmployeeRepository = controller.EmployeeService.Repository,
                PayrollRepository = controller.PayrollService.Repository,
                LookupSetRepository = controller.RegulationLookupSetService.Repository,
                RegulationRepository = controller.RegulationService.Repository,
                CalendarRepository = controller.CalendarService.Repository,
                PayrollCalculatorProvider = controller.PayrollCalculatorProvider,
                WebhookDispatchService = controller.WebhookDispatchService,
                RegulationDate = setup.RegulationDate.ToUtc(),
                EvaluationDate = setup.EvaluationDate.ToUtc(),
                ScriptProvider = controller.ScriptProvider,
                AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout
            };

            switch (setup.CaseType)
            {
                case CaseType.Global:
                    return new(
                        globalCaseValueRepository: controller.PayrollService.GlobalCaseValueRepository,
                        settings: settings);
                case CaseType.National:
                    return new(
                        globalCaseValueRepository: controller.PayrollService.GlobalCaseValueRepository,
                        nationalCaseValueRepository: controller.PayrollService.NationalCaseValueRepository,
                        settings: settings);
                case CaseType.Company:
                    return new(
                        globalCaseValueRepository: controller.PayrollService.GlobalCaseValueRepository,
                        nationalCaseValueRepository: controller.PayrollService.NationalCaseValueRepository,
                        companyCaseValueRepository: controller.PayrollService.CompanyCaseValueRepository,
                        settings: settings);
                case CaseType.Employee:
                    if (setup.Employee == null)
                    {
                        throw new ArgumentNullException(nameof(setup.Employee));
                    }
                    return new(setup.Employee,
                        globalCaseValueRepository: controller.PayrollService.GlobalCaseValueRepository,
                        nationalCaseValueRepository: controller.PayrollService.NationalCaseValueRepository,
                        companyCaseValueRepository: controller.PayrollService.CompanyCaseValueRepository,
                        employeeCaseValueRepository: controller.PayrollService.EmployeeCaseValueRepository,
                        settings: settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(setup.CaseType), setup.CaseType, null);
            }
        }

        internal DerivedCaseCollector NewCaseCollector(CaseSetupSettings setup)
        {
            // server configuration
            var serverConfiguration = controller.Configuration.GetConfiguration<PayrollServerConfiguration>();

            var settings = new DerivedCaseToolSettings
            {
                DbContext = controller.Runtime.DbContext,
                Tenant = setup.Tenant,
                Culture = setup.Culture,
                Calendar = setup.Calendar,
                User = setup.User,
                Payroll = setup.Payroll,
                TaskRepository = controller.TaskService.Repository,
                LogRepository = controller.LogService.Repository,
                DivisionRepository = controller.DivisionService.Repository,
                EmployeeRepository = controller.EmployeeService.Repository,
                PayrollRepository = controller.PayrollService.Repository,
                LookupSetRepository = controller.RegulationLookupSetService.Repository,
                RegulationRepository = controller.RegulationService.Repository,
                CalendarRepository = controller.CalendarService.Repository,
                PayrollCalculatorProvider = controller.PayrollCalculatorProvider,
                WebhookDispatchService = controller.WebhookDispatchService,
                RegulationDate = setup.RegulationDate.ToUtc(),
                EvaluationDate = setup.EvaluationDate.ToUtc(),
                ClusterSetName = setup.ClusterSetName,
                ScriptProvider = controller.ScriptProvider,
                AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout
            };

            switch (setup.CaseType)
            {
                case CaseType.Global:
                    return new(
                        controller.PayrollService.GlobalCaseValueRepository,
                        settings);
                case CaseType.National:
                    return new(
                        controller.PayrollService.GlobalCaseValueRepository,
                        controller.PayrollService.NationalCaseValueRepository,
                        settings);
                case CaseType.Company:
                    return new(
                        controller.PayrollService.GlobalCaseValueRepository,
                        controller.PayrollService.NationalCaseValueRepository,
                        controller.PayrollService.CompanyCaseValueRepository,
                        settings);
                case CaseType.Employee:
                    if (setup.Employee == null)
                    {
                        throw new ArgumentNullException(nameof(setup.Employee));
                    }
                    return new(setup.Employee,
                        controller.PayrollService.GlobalCaseValueRepository,
                        controller.PayrollService.NationalCaseValueRepository,
                        controller.PayrollService.CompanyCaseValueRepository,
                        controller.PayrollService.EmployeeCaseValueRepository,
                        settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(setup.CaseType), setup.CaseType, null);
            }
        }

        internal DerivedCaseBuilder NewCaseBuilder(CaseSetupSettings setup)
        {
            // server configuration
            var serverConfiguration = controller.Configuration.GetConfiguration<PayrollServerConfiguration>();

            var settings = new DerivedCaseToolSettings
            {
                DbContext = controller.Runtime.DbContext,
                Tenant = setup.Tenant,
                Culture = setup.Culture,
                Calendar = setup.Calendar,
                User = setup.User,
                Payroll = setup.Payroll,
                TaskRepository = controller.TaskService.Repository,
                LogRepository = controller.LogService.Repository,
                DivisionRepository = controller.DivisionService.Repository,
                EmployeeRepository = controller.EmployeeService.Repository,
                PayrollRepository = controller.PayrollService.Repository,
                LookupSetRepository = controller.RegulationLookupSetService.Repository,
                RegulationRepository = controller.RegulationService.Repository,
                CalendarRepository = controller.CalendarService.Repository,
                PayrollCalculatorProvider = controller.PayrollCalculatorProvider,
                WebhookDispatchService = controller.WebhookDispatchService,
                RegulationDate = setup.RegulationDate.ToUtc(),
                EvaluationDate = setup.EvaluationDate.ToUtc(),
                ClusterSetName = setup.ClusterSetName,
                ScriptProvider = controller.ScriptProvider,
                AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout
            };

            switch (setup.CaseType)
            {
                case CaseType.Global:
                    return new(
                        controller.PayrollService.GlobalCaseValueRepository,
                        settings);
                case CaseType.National:
                    return new(
                        controller.PayrollService.GlobalCaseValueRepository,
                        controller.PayrollService.NationalCaseValueRepository,
                        settings);
                case CaseType.Company:
                    return new(
                        controller.PayrollService.GlobalCaseValueRepository,
                        controller.PayrollService.NationalCaseValueRepository,
                        controller.PayrollService.CompanyCaseValueRepository,
                        settings);
                case CaseType.Employee:
                    if (setup.Employee == null)
                    {
                        throw new ArgumentNullException(nameof(setup.Employee));
                    }
                    return new(setup.Employee,
                        controller.PayrollService.GlobalCaseValueRepository,
                        controller.PayrollService.NationalCaseValueRepository,
                        controller.PayrollService.CompanyCaseValueRepository,
                        controller.PayrollService.EmployeeCaseValueRepository,
                        settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(setup.CaseType), setup.CaseType, null);
            }
        }

        internal IRegulationLookupProvider NewRegulationLookupProvider(IDbContext context, Tenant tenant, Payroll payroll, DateTime? regulationDate = null, DateTime? evaluationDate = null)
        {
            var currentEvaluationDate = controller.CurrentEvaluationDate;
            regulationDate ??= currentEvaluationDate;
            evaluationDate ??= currentEvaluationDate;

            // new lookup provider
            return new RegulationLookupProvider(
                dbContext: context,
                payrollRepository: controller.PayrollService.Repository,
                payrollQuery: new()
                {
                    TenantId = tenant.Id,
                    PayrollId = payroll.Id,
                    RegulationDate = regulationDate.Value.ToUtc(),
                    EvaluationDate = evaluationDate.Value.ToUtc()
                },
                regulationRepository: controller.RegulationService.Repository,
                lookupSetRepository: controller.RegulationLookupSetService.Repository);
        }
    }
}