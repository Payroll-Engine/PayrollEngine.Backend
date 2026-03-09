using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Service for previewing payrun job results without persisting to the database.
/// </summary>
public class PayrunPreviewService(
    IDbContext dbContext,
    IConfiguration configuration,
    IScriptProvider scriptProvider,
    IUserRepository userRepository,
    IDivisionRepository divisionRepository,
    ITaskRepository taskRepository,
    ILogRepository logRepository,
    IEmployeeRepository employeeRepository,
    IGlobalCaseValueRepository globalCaseValueRepository,
    INationalCaseValueRepository nationalCaseValueRepository,
    ICompanyCaseValueRepository companyCaseValueRepository,
    IEmployeeCaseValueRepository employeeCaseValueRepository,
    IPayrunJobRepository payrunJobRepository,
    ILookupSetRepository lookupSetRepository,
    IRegulationRepository regulationRepository,
    IRegulationShareRepository regulationShareRepository,
    IPayrollRepository payrollRepository,
    IPayrollResultRepository payrollResultRepository,
    IPayrollConsolidatedResultRepository payrollConsolidatedResultRepository,
    IPayrollResultSetRepository payrollResultSetRepository,
    ICalendarRepository calendarRepository,
    IPayrollCalculatorProvider payrollCalculatorProvider,
    IWebhookDispatchService webhookDispatchService) : IPayrunPreviewService
{
    /// <inheritdoc />
    public async Task<PayrollResultSet> PreviewAsync(Tenant tenant, Payrun payrun,
        PayrunJobInvocation jobInvocation)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(payrun);
        ArgumentNullException.ThrowIfNull(jobInvocation);

        if (jobInvocation.EmployeeIdentifiers == null || jobInvocation.EmployeeIdentifiers.Count != 1)
        {
            throw new PayrunException("Preview requires exactly one employee identifier.");
        }

        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();

        var settings = new PayrunProcessorSettings
        {
            DbContext = dbContext,
            ScriptProvider = scriptProvider,
            UserRepository = userRepository,
            DivisionRepository = divisionRepository,
            TaskRepository = taskRepository,
            LogRepository = logRepository,
            EmployeeRepository = employeeRepository,
            GlobalCaseValueRepository = globalCaseValueRepository,
            NationalCaseValueRepository = nationalCaseValueRepository,
            CompanyCaseValueRepository = companyCaseValueRepository,
            EmployeeCaseValueRepository = employeeCaseValueRepository,
            PayrunJobRepository = payrunJobRepository,
            RegulationLookupSetRepository = lookupSetRepository,
            RegulationRepository = regulationRepository,
            RegulationShareRepository = regulationShareRepository,
            PayrollRepository = payrollRepository,
            PayrollResultRepository = payrollResultRepository,
            PayrollConsolidatedResultRepository = payrollConsolidatedResultRepository,
            PayrollResultSetRepository = payrollResultSetRepository,
            CalendarRepository = calendarRepository,
            PayrollCalculatorProvider = payrollCalculatorProvider,
            WebhookDispatchService = webhookDispatchService,
            FunctionLogTimeout = serverConfiguration.FunctionLogTimeout,
            AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout,
            // preview-specific settings
            Mode = PayrunProcessorMode.Preview,
            MaxParallelEmployees = 0,
            MaxRetroPayrunPeriods = 0,
            LogEmployeeTiming = false
        };

        var processor = new PayrunProcessor(tenant, payrun, settings);
        var result = await processor.ProcessPreview(jobInvocation);

        if (result.PayrunJob.JobStatus == PayrunJobStatus.Abort)
        {
            throw new PayrunException(
                $"Preview failed: {result.PayrunJob.Message}");
        }

        return result.ResultSet ?? new PayrollResultSet();
    }
}
