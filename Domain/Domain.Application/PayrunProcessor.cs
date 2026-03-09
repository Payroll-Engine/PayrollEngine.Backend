using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Executes a payrun job for a single payroll period across one or more employees.
/// <para>
/// <b>Processing Pipeline:</b>
/// <list type="number">
///   <item>Context setup (payroll, division, culture, calendar, calculator)</item>
///   <item>Job creation or resumption (sync vs. async pre-created)</item>
///   <item>Regulation, collector and wage type derivation</item>
///   <item>Employee resolution and filtering</item>
///   <item>Per-employee calculation (sequential or parallel)</item>
///   <item>Retro payrun processing (one level deep, per employee)</item>
///   <item>Incremental result pruning (optional)</item>
///   <item>Result persistence</item>
/// </list>
/// </para>
/// <para>
/// <b>Thread-safety:</b> When <c>MaxParallelEmployees &gt; 0</c>, employees are processed
/// in parallel using see Parallel.ForEachAsync. Each employee gets its own
/// <c>PayrunEmployeeScope</c> for mutable state isolation. Shared immutable
/// context is provided via <c>PayrunContext</c>.
/// </para>
/// </summary>
public class PayrunProcessor : FunctionToolBase
{
    /// <summary>
    /// Preloaded objects that can be passed in from a parent payrun (e.g. retro jobs)
    /// to avoid redundant DB roundtrip. All members are optional; null means "load lazily".
    /// </summary>
    internal sealed class PayrunSetup
    {
        /// <summary>Preloaded payroll; null causes the processor to load it from the DB.</summary>
        internal Payroll Payroll { get; init; }
        /// <summary>Preloaded division; null causes the processor to load it from the DB.</summary>
        internal Division Division { get; init; }
        /// <summary>Explicit employee subset to process; null means all active employees of the division.</summary>
        internal System.Collections.Generic.List<Employee> Employees { get; init; }
        /// <summary>Shared global case value cache passed in from the parent payrun.</summary>
        internal CaseValueCache GlobalCaseValues { get; init; }
        /// <summary>Shared national case value cache passed in from the parent payrun.</summary>
        internal CaseValueCache NationalCaseValues { get; init; }
        /// <summary>Shared company case value cache passed in from the parent payrun.</summary>
        internal CaseValueCache CompanyCaseValues { get; init; }
        /// <summary>Preloaded employee case value cache; only valid when exactly one employee is processed.</summary>
        internal CaseValueCache EmployeeCaseValues { get; init; }
    }

    // global
    /// <summary>The owning tenant; immutable after construction.</summary>
    private Tenant Tenant { get; }
    /// <summary>The payrun definition; immutable after construction.</summary>
    private Payrun Payrun { get; }

    // internal
    private new PayrunProcessorSettings Settings => base.Settings as PayrunProcessorSettings;
    /// <summary>Builds the payrun context and creates/loads the job.</summary>
    private PayrunContextBuilder ContextBuilder { get; }
    /// <summary>Initialises regulations, providers, caches, collectors, and wage types.</summary>
    private PayrunRegulationInitializer RegulationInitializer { get; }
    /// <summary>Resolves the list of employees to process.</summary>
    private EmployeeResolver EmployeeResolver { get; }
    /// <summary>Processes all employees (sequential or parallel iteration).</summary>
    private PayrunEmployeesProcessor EmployeesProcessor { get; }

    /// <summary>True when the processor runs in preview mode (no DB writes).</summary>
    private bool IsPreview => Settings.Mode == PayrunProcessorMode.Preview;

    /// <summary>Log prefix indicating preview or live mode.</summary>
    private string ModeTag => IsPreview ? "[Preview] " : "";

    /// <summary>
    /// Initializes a new <see cref="PayrunProcessor"/>.
    /// </summary>
    /// <param name="tenant">The owning tenant.</param>
    /// <param name="payrun">The payrun definition to execute.</param>
    /// <param name="settings">Processor settings including repositories and configuration.</param>
    public PayrunProcessor(Tenant tenant, Payrun payrun, PayrunProcessorSettings settings) :
        base(settings)
    {
        // global
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));

        // shared services
        var resultProvider = new ResultProvider(Settings.PayrollResultRepository, Settings.PayrollConsolidatedResultRepository);
        var calculatorCache = new PayrollCalculatorCache(Settings.DbContext, Settings.CalendarRepository, Settings.PayrollCalculatorProvider);
        var resultPruner = new IncrementalResultPruner(Settings.DbContext, Settings.PayrollConsolidatedResultRepository);
        var retroProcessor = new PayrunRetroProcessor(Tenant, Payrun, Settings, IsPreview);
        var logWatch = Settings.FunctionLogTimeout != TimeSpan.Zero;

        // pipeline components
        ContextBuilder = new PayrunContextBuilder(Tenant, Payrun, calculatorCache, retroProcessor);
        RegulationInitializer = new PayrunRegulationInitializer(Settings, FunctionHost, resultProvider, Tenant, Payrun, logWatch);
        EmployeeResolver = new EmployeeResolver(Settings, FunctionHost, Tenant, Payrun, resultProvider);

        var employeeProcessor = new PayrunEmployeeProcessor(
            calculatorCache, resultPruner, retroProcessor, Settings, Tenant, Payrun, IsPreview, logWatch);
        EmployeesProcessor = new PayrunEmployeesProcessor(
            Settings, FunctionHost, resultProvider, Tenant, Payrun, employeeProcessor, logWatch,
            updateJobAsync: UpdateJobAsync,
            abortJobAsync: (job, message) => AbortJobAsync(job, message));
    }

    #region Process

    /// <summary>
    /// Executes the payrun for the given job invocation.
    /// </summary>
    /// <param name="jobInvocation">Invocation parameters including payrun id, period, and optional employee filter.</param>
    /// <returns>The completed or aborted <see cref="PayrunJob"/>.</returns>
    public async Task<PayrunJob> Process(PayrunJobInvocation jobInvocation) =>
        await Process(jobInvocation, new());

    /// <summary>
    /// Executes the payrun in preview mode for a single employee.
    /// No results are persisted to the database. The calculated result sets
    /// are returned as part of the <see cref="PayrunProcessResult"/>.
    /// </summary>
    /// <param name="jobInvocation">Invocation parameters. Must specify exactly one employee identifier.
    /// Any <c>RetroPayMode</c> is accepted to match production behavior.</param>
    /// <returns>The process result containing the job and in-memory result sets.</returns>
    /// <exception cref="PayrunException">Thrown when more than one employee is specified.</exception>
    /// <exception cref="PayrunPreviewRetroException">Thrown when retroactive calculation
    /// is triggered during preview. The caller should return HTTP 422.</exception>
    public async Task<PayrunProcessResult> ProcessPreview(PayrunJobInvocation jobInvocation)
    {
        if (jobInvocation.EmployeeIdentifiers == null || jobInvocation.EmployeeIdentifiers.Count != 1)
        {
            throw new PayrunException("Preview mode requires exactly one employee identifier.");
        }

        // preview accepts any RetroPayMode to match production behavior.
        // if retro calculation is actually triggered during processing,
        // a PayrunPreviewRetroException is thrown (see PayrunEmployeeProcessor).

        var payrunJob = await Process(jobInvocation, new());

        return new PayrunProcessResult
        {
            PayrunJob = payrunJob,
            ResultSet = EmployeesProcessor.PreviewResultSet
        };
    }

    /// <summary>
    /// Core process implementation. Accepts a pre-populated <see cref="PayrunSetup"/> to allow
    /// retro payrun jobs to reuse already-loaded context objects (payroll, division, case value caches).
    /// </summary>
    /// <param name="jobInvocation">Invocation parameters including payrun id, period, and optional employee filter.</param>
    /// <param name="setup">Pre-populated context objects from a parent payrun (retro scenario) or an empty
    /// instance for top-level invocations. Null members are loaded lazily from the database.</param>
    internal async Task<PayrunJob> Process(PayrunJobInvocation jobInvocation, PayrunSetup setup)
    {
        // precondition
        if (setup.EmployeeCaseValues != null && (setup.Employees == null || setup.Employees.Count != 1))
        {
            throw new ArgumentException("Invalid payrun employee setup", nameof(setup));
        }

        FunctionHost.LogLevel = jobInvocation.LogLevel;
        var processorRepositories = new PayrunProcessorRepositories(Settings, Tenant);

        var isRetro = setup.Payroll != null;
        var totalStopwatch = Stopwatch.StartNew();
        Log.Debug($"{ModeTag}Starting payrun {Payrun.Name} (id {Payrun.Id}) [{jobInvocation.Name}] " +
                  $"for tenant {Tenant.Identifier} (id {Tenant.Id})" +
                  $"{(isRetro ? " [retro]" : "")}");

        // Phase 1: build context (payroll, division, culture, calendar, calculator)
        Log.Trace($"{ModeTag}Phase 1: building context (payroll, division, culture, calendar, calculator)");
        var phaseStopwatch = Stopwatch.StartNew();
        var context = await ContextBuilder.BuildContextAsync(jobInvocation, setup, processorRepositories);
        Log.Trace($"{ModeTag}Phase 1 completed: payroll {context.Payroll.Name} (id {context.Payroll.Id}), " +
                  $"division {context.Division?.Name ?? "n/a"}, " +
                  $"period {jobInvocation.PeriodStart:d}..{context.PayrunJob?.PeriodEnd:d} " +
                  $"[{phaseStopwatch.ElapsedMilliseconds} ms]");

        // Phase 2: create or load job, resolve parent job, set evaluation dates
        Log.Trace($"{ModeTag}Phase 2: creating/loading job");
        phaseStopwatch.Restart();
        await ContextBuilder.CreateOrLoadJobAsync(jobInvocation, context, processorRepositories);
        var jobId = context.PayrunJob!.Id;
        Log.Trace($"{ModeTag}Phase 2 completed: job {jobId}, " +
                  $"status {context.PayrunJob.JobStatus}, " +
                  $"evaluation date {context.PayrunJob.EvaluationDate:d} " +
                  $"[{phaseStopwatch.ElapsedMilliseconds} ms]");

        // Phase 3: regulations, providers, caches, collectors, wage types
        Log.Trace($"{ModeTag}Phase 3: initializing regulations (job {jobId})");
        phaseStopwatch.Restart();
        var processorRegulation = await RegulationInitializer.InitializeAsync(context, setup, processorRepositories);
        var wageTypeCount = context.DerivedWageTypes.Count();
        if (wageTypeCount == 0)
        {
            return await AbortJobAsync(context.PayrunJob, $"No wage types available for payrun with id {Payrun}");
        }
        Log.Trace($"{ModeTag}Phase 3 completed: {wageTypeCount} wage types, " +
                  $"{context.SourceDerivedCollectors?.Count() ?? 0} collectors " +
                  $"[{phaseStopwatch.ElapsedMilliseconds} ms]");

        // Phase 4: employees
        Log.Trace($"{ModeTag}Phase 4: resolving employees (job {jobId})");
        phaseStopwatch.Restart();
        var employeesFromSetup = setup.Employees != null;
        var employees = setup.Employees ?? await EmployeeResolver.ResolveAsync(context, jobInvocation.EmployeeIdentifiers);
        if (employees.Count == 0)
        {
            return await AbortJobAsync(context.PayrunJob, $"No employees available for payrun with id {Payrun}");
        }
        Log.Trace($"{ModeTag}Phase 4 completed: {employees.Count} employees " +
                  $"(source: {(employeesFromSetup ? "setup" : "resolved")}) " +
                  $"[{phaseStopwatch.ElapsedMilliseconds} ms]");
        foreach (var employee in employees)
        {
            context.PayrunJob.Employees.Add(new() { EmployeeId = employee.Id });
        }

        // Phase 5: start and persist job
        Log.Trace($"{ModeTag}Phase 5: starting job {jobId}");
        phaseStopwatch.Restart();
        await StartJobAsync(context);
        Log.Trace($"{ModeTag}Phase 5 completed: job {jobId} started " +
                  $"[{phaseStopwatch.ElapsedMilliseconds} ms]");

        // Phase 6: validate regulation derivation chain
        // Ensures the chain is consistent (no circular references, no missing base regulations).
        // Distinct from the payroll-level validation in PayrunEmployeesProcessor,
        // which validates division assignment and period boundaries.
        Log.Trace($"{ModeTag}Phase 6: validating regulation chain (job {jobId})");
        phaseStopwatch.Restart();
        var validation = await new PayrollValidator(Settings.PayrollRepository).ValidateRegulations(
            context: Settings.DbContext,
            tenantId: Tenant.Id,
            payroll: context.Payroll,
            regulationDate: context.PayrunJob.PeriodEnd,
            evaluationDate: context.PayrunJob.EvaluationDate);
        if (!string.IsNullOrWhiteSpace(validation))
        {
            return await AbortJobAsync(context.PayrunJob, $"Regulation validation error: {validation}");
        }
        Log.Trace($"{ModeTag}Phase 6 completed: regulation chain valid " +
                  $"[{phaseStopwatch.ElapsedMilliseconds} ms]");

        // Phase 7: process all employees
        Log.Trace($"{ModeTag}Phase 7: processing {employees.Count} employees (job {jobId})");
        phaseStopwatch.Restart();
        var result = await EmployeesProcessor.ProcessAsync(
            setup: setup,
            context: context,
            processorRegulation: processorRegulation,
            processorRepositories: processorRepositories,
            employees: employees);
        Log.Trace($"{ModeTag}Phase 7 completed: employee processing finished " +
                  $"(job {jobId}, status {result.JobStatus}) " +
                  $"[{phaseStopwatch.ElapsedMilliseconds} ms]");

        totalStopwatch.Stop();
        Log.Debug($"{ModeTag}Payrun {Payrun.Name} (job {jobId}) [{jobInvocation.Name}] " +
                  $"completed with status {result.JobStatus}, " +
                  $"{employees.Count} employees, " +
                  $"total {totalStopwatch.ElapsedMilliseconds} ms");

        return result;
    }

    #endregion

    #region Job

    /// <summary>
    /// Sets the job start time, writes the start message, and persists the job
    /// (insert for new jobs, update for pre-created async jobs).
    /// </summary>
    private async Task StartJobAsync(PayrunContext context)
    {
        context.PayrunJob.JobStart = Date.Now;
        context.PayrunJob.Message =
            $"Started payrun calculation with payroll {context.Payroll.Id} on period {context.PayrunJob.PeriodName} for cycle {context.PayrunJob.CycleName}";
        Log.Debug(context.PayrunJob.Message);

        // preview mode: keep job in-memory only
        if (IsPreview)
        {
            return;
        }

        if (context.PayrunJob.Id == 0)
        {
            // New job (sync mode or retro jobs) - insert
            await Settings.PayrunJobRepository.CreateAsync(
                context: Settings.DbContext,
                parentId: Tenant.Id,
                item: context.PayrunJob);
        }
        else
        {
            // Pre-created job (async mode) - update
            await Settings.PayrunJobRepository.UpdateAsync(
                context: Settings.DbContext,
                parentId: Tenant.Id,
                item: context.PayrunJob);
        }
    }

    /// <summary>Persists the current state of <paramref name="payrunJob"/> to the database.
    /// Skipped in preview mode.</summary>
    private async Task UpdateJobAsync(PayrunJob payrunJob)
    {
        if (IsPreview)
        {
            return;
        }
        await Settings.PayrunJobRepository.UpdateAsync(Settings.DbContext, Tenant.Id, payrunJob);
    }

    /// <summary>
    /// Marks the job as aborted, logs the error, and persists the final state.
    /// </summary>
    /// <param name="payrunJob">The job to abort.</param>
    /// <param name="message">Human-readable abort reason written to <see cref="PayrunJob.Message"/>.</param>
    /// <param name="error">Optional exception written to <see cref="PayrunJob.ErrorMessage"/>.</param>
    private async Task<PayrunJob> AbortJobAsync(PayrunJob payrunJob, string message, Exception error = null)
    {
        // setup
        payrunJob.JobStatus = PayrunJobStatus.Abort;
        payrunJob.JobEnd = Date.Now;
        payrunJob.Message = message;
        if (error != null)
        {
            Log.Error(error, $"{ModeTag}Job {payrunJob.Id} aborted: {message}");
        }
        else
        {
            Log.Error($"{ModeTag}Job {payrunJob.Id} aborted: {message}");
        }
        payrunJob.ErrorMessage = error?.ToString();

        // preview mode: keep abort state in-memory only
        if (!IsPreview)
        {
            await Settings.PayrunJobRepository.UpdateAsync(Settings.DbContext, Tenant.Id, payrunJob);
        }

        return payrunJob;
    }

    #endregion
}