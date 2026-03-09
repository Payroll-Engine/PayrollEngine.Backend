//#define EMPLOYEE_SUMMARY_PERFORMANCE

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Iterates over all employees in a payrun job, running each through
/// <see cref="PayrunEmployeeProcessor"/> either sequentially or in parallel.
/// Handles payroll validation, payrun start/end scripts, progress tracking,
/// and deadlock retry with exponential backoff in parallel mode.
/// </summary>
internal sealed class PayrunEmployeesProcessor
{
    private PayrunProcessorSettings Settings { get; }
    private FunctionHost FunctionHost { get; }
    private IResultProvider ResultProvider { get; }
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }
    private PayrunEmployeeProcessor EmployeeProcessor { get; }
    private bool LogWatch { get; }

    /// <summary>
    /// The preview result set collected from the single-employee preview run.
    /// Only set when the processor runs in preview mode.
    /// </summary>
    internal PayrollResultSet PreviewResultSet { get; private set; }

    /// <summary>Job persistence delegate; skips DB write in preview mode.</summary>
    private Func<PayrunJob, Task> UpdateJobAsync { get; }
    /// <summary>Job abort delegate; marks the job as aborted and persists the final state.</summary>
    private Func<PayrunJob, string, Task<PayrunJob>> AbortJobAsync { get; }

#if EMPLOYEE_SUMMARY_PERFORMANCE
    /// <summary>Collects per-employee processing durations (ms) for summary statistics.</summary>
    private System.Collections.Concurrent.ConcurrentBag<long> PerfEmployeeTimings { get; set; }
#endif

    /// <summary>
    /// Initializes a new <see cref="PayrunEmployeesProcessor"/>.
    /// </summary>
    internal PayrunEmployeesProcessor(
        PayrunProcessorSettings settings,
        FunctionHost functionHost,
        IResultProvider resultProvider,
        Tenant tenant,
        Payrun payrun,
        PayrunEmployeeProcessor employeeProcessor,
        bool logWatch,
        Func<PayrunJob, Task> updateJobAsync,
        Func<PayrunJob, string, Task<PayrunJob>> abortJobAsync)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        FunctionHost = functionHost ?? throw new ArgumentNullException(nameof(functionHost));
        ResultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
        EmployeeProcessor = employeeProcessor ?? throw new ArgumentNullException(nameof(employeeProcessor));
        LogWatch = logWatch;
        UpdateJobAsync = updateJobAsync ?? throw new ArgumentNullException(nameof(updateJobAsync));
        AbortJobAsync = abortJobAsync ?? throw new ArgumentNullException(nameof(abortJobAsync));
    }

    /// <summary>
    /// Validates the payroll configuration (division, period boundaries), runs the payrun
    /// start script, then processes all employees either sequentially
    /// (<c>MaxParallelEmployees == 0</c>) or in parallel.
    /// </summary>
    /// <param name="setup">Preloaded shared objects passed through to each employee processor.</param>
    /// <param name="context">The fully initialised payrun context including job, payroll, and case value caches.</param>
    /// <param name="processorRegulation">Wage type and collector calculation logic.</param>
    /// <param name="processorRepositories">Repository facade for loading payroll-related entities.</param>
    /// <param name="employees">The resolved list of employees to process.</param>
    /// <returns>The completed or aborted <see cref="PayrunJob"/>.</returns>
    /// <remarks>
    /// The payroll-level validation here checks the payroll object itself (division assignment,
    /// period boundaries). This is distinct from the regulation validation in the orchestrator,
    /// which validates the regulation derivation chain (circular references, missing base regulations).
    /// </remarks>
    internal async Task<PayrunJob> ProcessAsync(PayrunProcessor.PayrunSetup setup, PayrunContext context,
        PayrunProcessorRegulation processorRegulation, PayrunProcessorRepositories processorRepositories,
        IList<Employee> employees)
    {
        try
        {
            // payroll-level validation:
            // Validates the payroll object itself against its division and the current period,
            // e.g. division assignment, period boundaries, and payroll configuration consistency.
            var validation = await processorRepositories.ValidatePayrollAsync(context.Payroll, context.Division,
                context.PayrunJob.GetEvaluationPeriod(), context.EvaluationDate);
            if (validation != null)
            {
                return await AbortJobAsync(context.PayrunJob, $"Payroll validation error: {validation}");
            }

            // performance measure
            var stopwatch = LogWatch ? System.Diagnostics.Stopwatch.StartNew() : null;

            // process scripts
            var processorScript = new PayrunProcessorScripts(
                functionHost: FunctionHost,
                settings: Settings,
                regulationProvider: context,
                resultProvider: ResultProvider,
                tenant: Tenant,
                payrun: Payrun);
            // payrun start script
            if (!processorScript.PayrunStart(context))
            {
                return await AbortJobAsync(context.PayrunJob, $"Payrun start failed for payrun with id {Payrun}");
            }

            if (stopwatch != null)
            {
                stopwatch.Stop();
                Log.Debug($"{Payrun} payrun start script: {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();
            }

            // update job with the total employee count
            context.PayrunJob.TotalEmployeeCount = employees.Count;
            await UpdateJobAsync(context.PayrunJob);

            // calculate payrun per employee – sequential or parallel depending on configuration
            var maxParallel = Settings.MaxParallelEmployees;
            var logTiming = Settings.LogEmployeeTiming;
            var employeeStopwatch = logTiming ? System.Diagnostics.Stopwatch.StartNew() : null;
#if EMPLOYEE_SUMMARY_PERFORMANCE
            PerfEmployeeTimings = new();
#endif
            if (maxParallel == 0)
            {
                await ProcessSequentialAsync(processorRegulation, processorScript, context, setup, employees, logTiming);
            }
            else
            {
                await ProcessParallelAsync(processorRegulation, processorScript, context, setup, employees, maxParallel, logTiming);
            }
            if (logTiming)
            {
                employeeStopwatch!.Stop();
                var avgMs = employees.Count > 0 ? employeeStopwatch.ElapsedMilliseconds / employees.Count : 0;
                var mode = maxParallel == 0 ? "sequential" : $"parallel (max. {(maxParallel == -1 ? Environment.ProcessorCount : maxParallel)} threads)";
                Log.Information($"{Payrun} employee processing completed: {mode}, {employees.Count} employee(s), total {employeeStopwatch.ElapsedMilliseconds} ms, avg {avgMs} ms/employee");
            }
#if EMPLOYEE_SUMMARY_PERFORMANCE
            if (PerfEmployeeTimings.Count > 0)
            {
                var sorted = PerfEmployeeTimings.OrderBy(x => x).ToArray();
                var p50 = sorted[sorted.Length / 2];
                var p95 = sorted[(int)(sorted.Length * 0.95)];
                var p99 = sorted[(int)(sorted.Length * 0.99)];
                Log.Information($"{Payrun} employee timing stats ({sorted.Length} employees): " +
                    $"min={sorted[0]}ms, p50={p50}ms, p95={p95}ms, p99={p99}ms, " +
                    $"max={sorted[^1]}ms, avg={sorted.Average():F0}ms");
            }
#endif

            // completed with failure(s)
            if (context.Errors.Any())
            {
                return await AbortJobAsync(context.PayrunJob, context.GetErrorMessages());
            }

            // payrun end script
            processorScript.PayrunEnd(context);

            // payrun job
            return context.PayrunJob;
        }
        catch (Exception exception)
        {
            var message = exception.GetBaseMessage();
            Log.Error(exception, message);
            return await AbortJobAsync(context.PayrunJob, $"Error in payrun: {message}");
        }
    }

    /// <summary>
    /// Sequential mode (default) – processes employees one by one, original behaviour.
    /// </summary>
    private async Task ProcessSequentialAsync(PayrunProcessorRegulation processorRegulation,
        PayrunProcessorScripts processorScript, PayrunContext context,
        PayrunProcessor.PayrunSetup setup, IList<Employee> employees, bool logTiming)
    {
        if (logTiming)
            Log.Information($"{Payrun} employee processing started: sequential mode, {employees.Count} employee(s)");

        foreach (var employee in employees)
        {
#if EMPLOYEE_SUMMARY_PERFORMANCE
            var singleStopwatch = System.Diagnostics.Stopwatch.StartNew();
#else
            var singleStopwatch = logTiming ? System.Diagnostics.Stopwatch.StartNew() : null;
#endif
            try
            {
                var employeeScope = new PayrunEmployeeScope(context);
                var previewResult = await EmployeeProcessor.ProcessAsync(
                    processorRegulation, processorScript, employee, employeeScope, setup);
                if (previewResult != null)
                {
                    PreviewResultSet = previewResult;
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, exception.GetBaseMessage());
                context.Errors.TryAdd(employee, exception);
            }
            if (logTiming)
                Log.Information($"{Payrun} employee {employee.Identifier} processed in {singleStopwatch!.ElapsedMilliseconds} ms");

#if EMPLOYEE_SUMMARY_PERFORMANCE
            if (singleStopwatch != null)
                PerfEmployeeTimings?.Add(singleStopwatch.ElapsedMilliseconds);
#endif

            // update payrun job progress
            // ProcessedEmployeeCount is progress-reporting only (UI/monitoring);
            // not used for business logic. Safe without synchronization in sequential mode.
            try
            {
                context.PayrunJob.ProcessedEmployeeCount++;
                Log.Trace($"Payrun calculated {context.PayrunJob.ProcessedEmployeeCount} employees");
                await UpdateJobAsync(context.PayrunJob);
            }
            catch (Exception exception)
            {
                Log.Error(exception, exception.GetBaseMessage());
                context.Errors.TryAdd(employee, exception);
                // continue with the next employee
            }
        }
    }

    /// <summary>
    /// Parallel mode – each employee gets its own <see cref="PayrunEmployeeScope"/> so
    /// mutable per-employee state is fully isolated between threads. Includes deadlock
    /// retry with exponential backoff.
    /// </summary>
    private async Task ProcessParallelAsync(PayrunProcessorRegulation processorRegulation,
        PayrunProcessorScripts processorScript, PayrunContext context,
        PayrunProcessor.PayrunSetup setup, IList<Employee> employees, int maxParallel, bool logTiming)
    {
        var degreeOfParallelism = maxParallel == -1
            ? Environment.ProcessorCount
            : maxParallel;

        if (logTiming)
            Log.Information($"{Payrun} employee processing started: parallel mode, {employees.Count} employee(s), max. {degreeOfParallelism} thread(s) (ProcessorCount={Environment.ProcessorCount})");

        var processedCount = 0;
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };

        // ReSharper disable once AllUnderscoreLocalParameterName
        await Parallel.ForEachAsync(employees, parallelOptions, async (employee, _) =>
        {
#if EMPLOYEE_SUMMARY_PERFORMANCE
            var singleStopwatch = System.Diagnostics.Stopwatch.StartNew();
#else
            var singleStopwatch = logTiming ? System.Diagnostics.Stopwatch.StartNew() : null;
#endif
            const int maxDeadlockRetries = 3;
            for (var attempt = 1; attempt <= maxDeadlockRetries; attempt++)
            {
                try
                {
                    var employeeScope = new PayrunEmployeeScope(context);
                    var previewResult = await EmployeeProcessor.ProcessAsync(
                        processorRegulation, processorScript, employee, employeeScope, setup);
                    if (previewResult != null)
                    {
                        PreviewResultSet = previewResult;
                    }
                    break; // success
                }
                catch (Exception exception) when (attempt < maxDeadlockRetries && IsDeadlockException(exception))
                {
                    // SQL Server deadlock (error 1205): retry after randomized backoff
                    var delayMs = attempt * 200 + Random.Shared.Next(50, 200);
                    Log.Warning($"{Payrun} employee {employee.Identifier}: deadlock detected (attempt {attempt}/{maxDeadlockRetries}), retrying in {delayMs} ms");
                    await Task.Delay(delayMs, _);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, exception.GetBaseMessage());
                    context.Errors.TryAdd(employee, exception);
                    break;
                }
            }
            if (logTiming)
                Log.Information($"{Payrun} employee {employee.Identifier} processed in {singleStopwatch!.ElapsedMilliseconds} ms");

#if EMPLOYEE_SUMMARY_PERFORMANCE
            PerfEmployeeTimings?.Add(singleStopwatch.ElapsedMilliseconds);
#endif

            // Thread-safe progress update
            var current = Interlocked.Increment(ref processedCount);
            Log.Trace($"Payrun calculated {current} employees");

            // Persist progress every 10 employees or when all are done,
            // to avoid flooding the DB with one update per employee.
            // ProcessedEmployeeCount is progress-only; a slightly stale snapshot is acceptable.
            if (current % 10 == 0 || current == employees.Count)
            {
                context.PayrunJob.ProcessedEmployeeCount = current;
                try
                {
                    await UpdateJobAsync(context.PayrunJob);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, exception.GetBaseMessage());
                }
            }
        });
    }

    /// <summary>
    /// Determines whether the given exception (or any of its inner exceptions)
    /// represents a SQL Server deadlock (error 1205). This is used by the parallel
    /// employee processing loop to decide whether a retry is appropriate.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><c>true</c> if the exception chain contains a SQL Server deadlock error.</returns>
    private static bool IsDeadlockException(Exception exception)
    {
        // walk the full exception chain (InnerException + AggregateException)
        for (var ex = exception; ex != null; ex = ex.InnerException)
        {
            // SQL Server deadlock: error number 1205
            // Microsoft.Data.SqlClient.SqlException inherits from DbException;
            // check by type name to avoid a hard assembly dependency.
            if (ex.GetType().Name == "SqlException" &&
                ex.GetType().GetProperty("Number")?.GetValue(ex) is int errorNumber &&
                errorNumber == 1205)
            {
                return true;
            }

            // fallback: check the message text (covers wrapped exceptions)
            if (ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // unwrap AggregateException
            if (ex is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                {
                    if (IsDeadlockException(inner))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
