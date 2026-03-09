using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Scripting.Controller;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Resolves the list of employees to process for a payrun job.
/// <para>
/// When employee identifiers are specified, only those employees are returned
/// (validated for active status and division membership). Otherwise, all active
/// employees of the payroll division are returned, optionally filtered by the
/// payrun's <c>EmployeeAvailableExpression</c> script.
/// </para>
/// </summary>
internal sealed class EmployeeResolver
{
    private PayrunProcessorSettings Settings { get; }
    private FunctionHost FunctionHost { get; }
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }
    private IResultProvider ResultProvider { get; }

    /// <summary>
    /// Initializes a new <see cref="EmployeeResolver"/>.
    /// </summary>
    /// <param name="settings">Processor settings including repositories and configuration.</param>
    /// <param name="functionHost">Function host for script execution.</param>
    /// <param name="tenant">The owning tenant.</param>
    /// <param name="payrun">The payrun definition.</param>
    /// <param name="resultProvider">Provides access to payroll results for script execution.</param>
    internal EmployeeResolver(
        PayrunProcessorSettings settings,
        FunctionHost functionHost,
        Tenant tenant,
        Payrun payrun,
        IResultProvider resultProvider)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        FunctionHost = functionHost ?? throw new ArgumentNullException(nameof(functionHost));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
        ResultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
    }

    /// <summary>
    /// Resolves the list of employees to process. When <paramref name="employeeIdentifiers"/> is
    /// provided, only those employees are returned (each validated for active status and division
    /// membership). Otherwise, all active employees of the payroll division are returned, optionally
    /// filtered by the payrun's <c>EmployeeAvailableExpression</c> script.
    /// </summary>
    /// <param name="context">The payrun context with division, payroll, and case value caches.</param>
    /// <param name="employeeIdentifiers">Optional list of employee identifiers to resolve; null means all active employees.</param>
    /// <returns>The resolved and filtered list of employees.</returns>
    /// <exception cref="PayrunException">Thrown when a specified employee identifier is unknown,
    /// inactive, or not in the payroll division.</exception>
    internal async Task<List<Employee>> ResolveAsync(PayrunContext context, List<string> employeeIdentifiers)
    {
        List<Employee> employees;

        // selected employees by identifier
        if (employeeIdentifiers != null && employeeIdentifiers.Any())
        {
            employees = [];
            foreach (var employeeIdentifier in employeeIdentifiers)
            {
                var query = new DivisionQuery
                {
                    Status = ObjectStatus.Active,
                    DivisionId = context.Division.Id,
                    Filter = $"{nameof(Employee.Identifier)} eq '{employeeIdentifier}'"
                };
                var selectedEmployees = (await Settings.EmployeeRepository.QueryAsync(Settings.DbContext, Tenant.Id, query)).ToList();
                if (selectedEmployees.Count != 1)
                {
                    throw new PayrunException($"Unknown employee with identifier {employeeIdentifier}");
                }
                var employee = selectedEmployees.First();
                // status
                if (employee.Status != ObjectStatus.Active)
                {
                    throw new PayrunException(
                        $"Payrun on inactive employee with identifier {employeeIdentifier} ({employee.Status})");
                }
                // division
                if (!employee.InDivision(context.Division.Name))
                {
                    throw new PayrunException(
                        $"Employee with identifier {employeeIdentifier} is not in division {context.Division.Name}");
                }

                employees.Add(employee);
            }

            return employees;
        }

        // all active employees from the division
        var allQuery = new DivisionQuery
        {
            Status = ObjectStatus.Active,
            DivisionId = context.Division.Id
        };
        employees = (await Settings.EmployeeRepository.QueryAsync(Settings.DbContext, Tenant.Id, allQuery)).ToList();

        // employee available expression
        if (employees.Any() && !string.IsNullOrWhiteSpace(Payrun.EmployeeAvailableExpression))
        {
            employees = FilterAvailableEmployees(context, employees);
        }

        Log.Trace($"Payrun with {employees.Count} employees");
        return employees;
    }

    /// <summary>
    /// Executes the payrun's <c>EmployeeAvailableExpression</c> script against each employee
    /// and returns only those for which the script returns <c>true</c>.
    /// </summary>
    private List<Employee> FilterAvailableEmployees(PayrunContext context, List<Employee> employees)
    {
        var availableEmployees = new List<Employee>();

        var scriptController = new PayrunScriptController();
        foreach (var employee in employees)
        {
            // employee case values
            var employeeCaseValueSet = new CaseValueCacheFactory(
                    Settings.DbContext, context.PayrunJob.DivisionId, context.PayrunJob.EvaluationDate, context.PayrunJob.Forecast)
                .Create(Settings.EmployeeCaseValueRepository, employee.Id);

            var caseValueProvider = new CaseValueProvider(
                settings: new()
                {
                    DbContext = Settings.DbContext,
                    Calculator = context.Calculator,
                    CaseFieldProvider = context.CaseFieldProvider,
                    EvaluationPeriod = context.EvaluationPeriod,
                    EvaluationDate = context.EvaluationDate,
                    RetroDate = context.RetroDate,
                },
                globalCaseValueRepository: context.GlobalCaseValues,
                nationalCaseValueRepository: context.NationalCaseValues,
                companyCaseValueRepository: context.CompanyCaseValues,
                employeeCaseValueRepository: employeeCaseValueSet,
                employee: employee);
            var isAvailable = scriptController.IsEmployeeAvailable(new()
            {
                DbContext = Settings.DbContext,
                PayrollCulture = context.PayrollCulture,
                Namespace = null,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = context.User,
                Payroll = context.Payroll,
                Payrun = Payrun,
                PayrunJob = context.PayrunJob,
                ParentPayrunJob = context.ParentPayrunJob,
                ExecutionPhase = PayrunExecutionPhase.Setup,
                RegulationProvider = context,
                ResultProvider = ResultProvider,
                CaseValueProvider = caseValueProvider,
                RegulationLookupProvider = context.RegulationLookupProvider,
                RuntimeValueProvider = context.RuntimeValueProvider,
                DivisionRepository = Settings.DivisionRepository,
                EmployeeRepository = Settings.EmployeeRepository,
                CalendarRepository = Settings.CalendarRepository,
                PayrollCalculatorProvider = Settings.PayrollCalculatorProvider,
                WebhookDispatchService = Settings.WebhookDispatchService
            });
            if (isAvailable.HasValue && isAvailable.Value)
            {
                availableEmployees.Add(employee);
            }
            else
            {
                Log.Trace($"Ignoring employee {employee}");
            }
        }

        return availableEmployees;
    }
}
