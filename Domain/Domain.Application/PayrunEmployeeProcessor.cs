//#define EMPLOYEE_PERFORMANCE

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Processes a single employee within a payrun job: runs employee start/end scripts,
/// calculates all wage types and collectors, handles retro payrun jobs, and persists
/// (or collects) the payroll result set.
/// <para>
/// The wage type loop supports execution restarts when a wage type script sets the
/// restart flag, bounded by <see cref="SystemSpecification.PayrunMaxExecutionCount"/>
/// to prevent infinite loops. If a Guard wage type calls AbortExecution(), the loop
/// stops immediately for this employee; CollectorEnd and PayrunEmployeeEnd still run.
/// </para>
/// </summary>
internal sealed class PayrunEmployeeProcessor
{
    private PayrollCalculatorCache CalculatorCache { get; }
    private IncrementalResultPruner ResultPruner { get; }
    private PayrunRetroProcessor RetroProcessor { get; }
    private PayrunProcessorSettings Settings { get; }
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }
    private bool IsPreview { get; }
    private bool LogWatch { get; }

    /// <summary>Serializes (or limits parallelism of) the result persistence phase across
    /// parallel employee threads. During bulk inserts, SQL Server acquires page-level X locks
    /// on multiple indexes of result tables (e.g. WageTypeCustomResult). These locks are held
    /// until the ambient TransactionScope commits. Without serialization, threads deadlock when
    /// they hold X locks on different pages/indexes and wait for each other.
    /// The semaphore must wrap the entire persist call (not individual bulk inserts)
    /// because the TransactionScope spans all child bulk inserts.
    /// Configurable via <see cref="PayrunProcessorSettings.MaxParallelPersist"/>:
    /// 1 = fully serialized (default), N = limited parallelism (deadlock risk increases with N).</summary>
    private readonly SemaphoreSlim PersistSemaphore;

    /// <summary>
    /// Initializes a new <see cref="PayrunEmployeeProcessor"/>.
    /// </summary>
    internal PayrunEmployeeProcessor(
        PayrollCalculatorCache calculatorCache,
        IncrementalResultPruner resultPruner,
        PayrunRetroProcessor retroProcessor,
        PayrunProcessorSettings settings,
        Tenant tenant,
        Payrun payrun,
        bool isPreview,
        bool logWatch)
    {
        CalculatorCache = calculatorCache ?? throw new ArgumentNullException(nameof(calculatorCache));
        ResultPruner = resultPruner ?? throw new ArgumentNullException(nameof(resultPruner));
        RetroProcessor = retroProcessor ?? throw new ArgumentNullException(nameof(retroProcessor));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
        IsPreview = isPreview;
        LogWatch = logWatch;
        var maxParallelPersist = Math.Max(1, settings.MaxParallelPersist);
        PersistSemaphore = new SemaphoreSlim(maxParallelPersist, maxParallelPersist);
    }

    /// <summary>
    /// Processes a single employee: runs the employee start/end scripts, calculates wage types
    /// and collectors, handles retro payrun jobs, and persists the payroll result set.
    /// On failure all retro jobs created for this employee are cleaned up.
    /// </summary>
    /// <param name="processorRegulation">Wage type and collector calculation logic.</param>
    /// <param name="processorScripts">Payrun and employee lifecycle script execution.</param>
    /// <param name="employee">The employee to process.</param>
    /// <param name="scope">The per-employee mutable execution scope (isolated per thread in parallel mode).</param>
    /// <param name="setup">Preloaded shared objects (payroll, case value caches, etc.).</param>
    /// <returns>The payroll result set in preview mode; <c>null</c> in persist mode.</returns>
    /// <remarks>
    /// Retro processing is one level deep: retro jobs are created with
    /// <see cref="RetroPayMode.None"/> so that the recursive process call
    /// does not spawn further retro jobs. After all retro periods are processed,
    /// the current period is recalculated in <see cref="PayrunExecutionPhase.Reevaluation"/> to
    /// incorporate any values changed by the retro jobs.
    /// If an exception occurs, all retro jobs created for this employee are cleaned up
    /// (deleted from the database) before the exception is re-thrown.
    /// </remarks>
    internal async Task<PayrollResultSet> ProcessAsync(PayrunProcessorRegulation processorRegulation,
        PayrunProcessorScripts processorScripts, Employee employee, PayrunEmployeeScope scope,
        PayrunProcessor.PayrunSetup setup)
    {
        var retroJobs = new List<PayrunJob>();
        PayrollResultSet previewResult = null;

        Log.Trace($"Payrun processing employee {employee}");
        try
        {
#if EMPLOYEE_PERFORMANCE
            var perfWatch = System.Diagnostics.Stopwatch.StartNew();
#endif

            var employeeCaseValueSet = setup.EmployeeCaseValues ??
                new CaseValueCacheFactory(Settings.DbContext, scope.Division.Id, scope.EvaluationDate, scope.PayrunJob.Forecast)
                    .Create(Settings.EmployeeCaseValueRepository, employee.Id);

#if EMPLOYEE_PERFORMANCE
            perfWatch.Stop();
            var perfLoadCaseValuesMs = perfWatch.ElapsedMilliseconds;
            perfWatch.Restart();
#endif

            // resolve the cycle cache cluster set and collect matching WageType numbers for pre-loading
            var cycleClusterSet = string.IsNullOrWhiteSpace(scope.Payroll.ClusterSet?.ClusterSetWageTypeCycle)
                ? null
                : scope.Payroll.GetClusterSet(scope.Payroll.ClusterSet.ClusterSetWageTypeCycle);
            var cycleNumbers = cycleClusterSet == null
                ? []
                : scope.DerivedWageTypes
                    .Where(g => g.AvailableCluster(cycleClusterSet))
                    .Select(g => g.Key)
                    .ToList();
            await LoadWageTypeCycleCacheAsync(scope, employee, cycleNumbers);

            // resolve the Cons cluster set and collect matching WageType numbers for pre-loading
            var consClusterSet = string.IsNullOrWhiteSpace(scope.Payroll.ClusterSet?.ClusterSetWageTypeCons)
                ? null
                : scope.Payroll.GetClusterSet(scope.Payroll.ClusterSet.ClusterSetWageTypeCons);
            var consNumbers = consClusterSet == null
                ? []
                : scope.DerivedWageTypes
                    .Where(g => g.AvailableCluster(consClusterSet))
                    .Select(g => g.Key)
                    .ToList();
            await LoadWageTypeConsCacheAsync(scope, employee, consNumbers);

            // value provider
            var caseValueProvider = new CaseValueProvider(
                settings: new()
                {
                    DbContext = Settings.DbContext,
                    Calculator = scope.Calculator,
                    CaseFieldProvider = scope.CaseFieldProvider,
                    EvaluationPeriod = scope.EvaluationPeriod,
                    EvaluationDate = scope.EvaluationDate,
                    RetroDate = scope.RetroDate
                },
                globalCaseValueRepository: scope.GlobalCaseValues,
                nationalCaseValueRepository: scope.NationalCaseValues,
                companyCaseValueRepository: scope.CompanyCaseValues,
                employeeCaseValueRepository: employeeCaseValueSet,
                employee: employee);

            // employee start function
            if (!processorScripts.EmployeeStart(caseValueProvider, scope))
            {
                return null;
            }

#if EMPLOYEE_PERFORMANCE
            perfWatch.Stop();
            var perfEmployeeStartMs = perfWatch.ElapsedMilliseconds;
            perfWatch.Restart();
#endif

            // current period results
            // -> execution phase setup
            var employeeResults = await CalculateAsync(processorRegulation, employee,
                caseValueProvider, scope, PayrunExecutionPhase.Setup);
            var payrollResult = employeeResults.PayrollResult;

#if EMPLOYEE_PERFORMANCE
            perfWatch.Stop();
            var perfCalculateSetupMs = perfWatch.ElapsedMilliseconds;
            var perfRetroMs = 0L;
            var perfRetroPeriodCount = 0;
            var perfReevaluationMs = 0L;
#endif

            if (!payrollResult.CollectorResults.Any() && !payrollResult.WageTypeResults.Any())
            {
                scope.PayrunJob.Message = "No results available";
                Log.Debug($"Payrun job {scope.PayrunJob.Name} without results");
                return null;
            }

            // payrun retro jobs
            var payrunRetroJobs = employeeResults.RetroPayrunJobs;
            var retroDate = employeeResults.RetroCaseValue?.Start;
            if (scope.PayrunJob.RetroPayMode != RetroPayMode.None &&
                (retroDate != null || payrunRetroJobs != null && payrunRetroJobs.Any()))
            {
                // preview mode: retro calculation is not possible because
                // results are not persisted between invocations
                if (IsPreview)
                {
                    throw new PayrunPreviewRetroException(
                        $"Preview requires retroactive calculation for employee {employee.Identifier} " +
                        $"(retro date: {retroDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "script-triggered"}). " +
                        "Preview mode cannot perform retro calculations because results are not persisted.",
                        employee.Identifier,
                        retroDate);
                }

                var effectiveRetroDate = RetroProcessor.ResolveEffectiveRetroDate(
                    retroDate, payrunRetroJobs, scope.PayrunJob, scope.Calculator);

                if (effectiveRetroDate.HasValue)
                {
#if EMPLOYEE_PERFORMANCE
                    perfWatch.Restart();
#endif
                    retroJobs = await RetroProcessor.ProcessRetroPeriodsAsync(
                        effectiveRetroDate.Value, employee, scope, employeeCaseValueSet, payrunRetroJobs);
#if EMPLOYEE_PERFORMANCE
                    perfWatch.Stop();
                    perfRetroMs = perfWatch.ElapsedMilliseconds;
                    perfRetroPeriodCount = retroJobs.Count;
#endif
                }

                // retro jobs may changed base calculation values: recalculate the current period
                // -> execution phase reevaluations
                if (retroJobs.Count > 0)
                {
                    // reset employee runtime values
                    scope.RuntimeValueProvider.EmployeeValues.Clear();

                    // retro jobs may have changed prior-period results: invalidate and reload cycle cache
                    scope.WageTypeCycleCache = null;
                    await LoadWageTypeCycleCacheAsync(scope, employee, cycleNumbers);

                    // retro jobs may have changed consolidated results: invalidate and reload Cons cache
                    scope.WageTypeConsCache = null;
                    await LoadWageTypeConsCacheAsync(scope, employee, consNumbers);

#if EMPLOYEE_PERFORMANCE
                    perfWatch.Restart();
#endif
                    // reevaluate current period results
                    employeeResults = await CalculateAsync(processorRegulation, employee,
                        caseValueProvider, scope, PayrunExecutionPhase.Reevaluation);
                    payrollResult = employeeResults.PayrollResult;
#if EMPLOYEE_PERFORMANCE
                    perfWatch.Stop();
                    perfReevaluationMs = perfWatch.ElapsedMilliseconds;
#endif
                }
            }

            // result tags for retro jobs
            if (scope.RetroPayrunJobs != null && scope.RetroPayrunJobs.Any())
            {
                foreach (var retroJob in scope.RetroPayrunJobs)
                {
                    // only retro jobs from the current period or
                    // retro jobs between the retro parent job period and the current job period
                    if (scope.EvaluationPeriod.IsWithinOrAfter(retroJob.ScheduleDate))
                    {
                        payrollResult.AddTags(retroJob.ResultTags);
                    }
                }
            }

            // performance measure
            var stopwatch = LogWatch ? System.Diagnostics.Stopwatch.StartNew() : null;

#if EMPLOYEE_PERFORMANCE
            var perfSemaphoreWaitMs = 0L;
            var perfPersistMs = 0L;
            perfWatch.Restart();
#endif

            // store current period results by payrun job and employee
            if (scope.StoreEmptyResults || !payrollResult.IsEmpty())
            {
                if (IsPreview)
                {
                    // preview mode: keep in-memory (single employee)
                    previewResult = payrollResult;
                }
                else
                {
                    // serialize persistence to prevent cross-index page-lock deadlocks
                    // during parallel processing (X locks held until TransactionScope commits)
                    await PersistSemaphore.WaitAsync();
#if EMPLOYEE_PERFORMANCE
                    perfWatch.Stop();
                    perfSemaphoreWaitMs = perfWatch.ElapsedMilliseconds;
                    perfWatch.Restart();
#endif
                    try
                    {
                        await Settings.PayrollResultSetRepository.CreateAsync(Settings.DbContext, Tenant.Id, payrollResult);
                    }
                    finally
                    {
                        PersistSemaphore.Release();
                    }
#if EMPLOYEE_PERFORMANCE
                    perfWatch.Stop();
                    perfPersistMs = perfWatch.ElapsedMilliseconds;
#endif
                }
            }

            if (stopwatch != null)
            {
                stopwatch.Stop();
                Log.Debug($"{Payrun} store results: {stopwatch.ElapsedMilliseconds} ms");
            }

#if EMPLOYEE_PERFORMANCE
            perfWatch.Restart();
#endif

            // employee end function
            processorScripts.EmployeeEnd(caseValueProvider, scope);

#if EMPLOYEE_PERFORMANCE
            perfWatch.Stop();
            var perfEmployeeEndMs = perfWatch.ElapsedMilliseconds;
            Log.Information($"{Payrun} employee {employee.Identifier} perf: " +
                $"caseValues={perfLoadCaseValuesMs}ms, empStart={perfEmployeeStartMs}ms, " +
                $"calculate={perfCalculateSetupMs}ms, retro={perfRetroMs}ms ({perfRetroPeriodCount} periods), " +
                $"reeval={perfReevaluationMs}ms, semWait={perfSemaphoreWaitMs}ms, " +
                $"persist={perfPersistMs}ms, empEnd={perfEmployeeEndMs}ms");
#endif
        }
        catch
        {
            // retro job cleanup: delete all retro jobs created for this employee
            await RetroProcessor.CleanupRetroJobsAsync(retroJobs, employee, scope.PayrunJob);
            throw;
        }

        return previewResult;
    }

    /// <summary>
    /// Bulk-loads consolidated results (with retro-merge) for all WageTypes tagged via
    /// <c>Payroll.ClusterSet.ClusterSetWageTypeCons</c> and stores them in
    /// <paramref name="scope"/>.<see cref="PayrunEmployeeScope.WageTypeConsCache"/>.
    /// No-op when <paramref name="consNumbers"/> is empty or in the first period of the cycle.
    /// Uses the existing <c>GetConsolidatedWageTypeResults</c> SP with all WageType numbers
    /// and all completed period-start hashes in a single round-trip.
    /// </summary>
    private async Task LoadWageTypeConsCacheAsync(
        PayrunEmployeeScope scope, Employee employee, IList<decimal> consNumbers)
    {
        if (consNumbers.Count == 0)
        {
            return;
        }

        // first period of cycle: no prior results exist
        if (scope.PayrunJob.CycleStart >= scope.PayrunJob.PeriodStart)
        {
            scope.WageTypeConsCache = null;
            return;
        }

        // enumerate all completed period starts using the same logic as GetConsolidatedPeriodStarts
        // in the runtime — ensures the cache covers exactly the periods the scripts would query
        var completedPeriodStarts = new List<DateTime>();
        var period = scope.Calculator.GetPayrunPeriod(scope.PayrunJob.CycleStart);
        while (period.Start < scope.PayrunJob.PeriodStart)
        {
            completedPeriodStarts.Add(period.Start);
            period = period.GetPayrollPeriod(period.Start, 1);
        }

        if (completedPeriodStarts.Count == 0)
        {
            scope.WageTypeConsCache = null;
            return;
        }

        var results = await Settings.PayrollConsolidatedResultRepository.GetWageTypeResultsAsync(
            Settings.DbContext,
            new ConsolidatedWageTypeResultQuery
            {
                TenantId           = Tenant.Id,
                EmployeeId         = employee.Id,
                PeriodStarts       = completedPeriodStarts,
                WageTypeNumbers    = consNumbers,
                JobStatus          = PayrunJobStatus.Complete,
                Forecast           = scope.PayrunJob.Forecast,
                EvaluationDate     = scope.EvaluationDate,
                NoRetro            = false,
                ExcludeParentJobId = scope.ParentPayrunJob?.Id
            });

        scope.WageTypeConsCache = new WageTypeConsCache(
            periodMoment:    scope.PayrunJob.CycleStart,
            wageTypeNumbers: consNumbers,
            results:         results);
    }

    /// <summary>
    /// Bulk-loads WageType results for the current cycle for all WageTypes tagged via
    /// <c>Payroll.ClusterSet.ClusterSetWageTypeCycle</c> and stores them in
    /// <paramref name="scope"/>.<see cref="PayrunEmployeeScope.WageTypeCycleCache"/>.
    /// No-op when <paramref name="cycleNumbers"/> is empty or in the first period of the cycle.
    /// Works for any calendar cycle type (annual, quarterly, monthly, etc.).
    /// </summary>
    private async Task LoadWageTypeCycleCacheAsync(
        PayrunEmployeeScope scope, Employee employee, IList<decimal> cycleNumbers)
    {
        if (cycleNumbers.Count == 0)
        {
            return;
        }

        // first period of cycle: no prior results exist
        if (scope.PayrunJob.CycleStart >= scope.PayrunJob.PeriodStart)
        {
            scope.WageTypeCycleCache = null;
            return;
        }

        var previousPeriodEnd = scope.PayrunJob.PeriodStart.AddDays(-1);
        var results = await Settings.PayrollResultRepository.GetWageTypeResultsAsync(
            Settings.DbContext,
            new WageTypeResultQuery
            {
                TenantId = Tenant.Id,
                EmployeeId = employee.Id,
                Period = new DatePeriod(scope.PayrunJob.CycleStart, previousPeriodEnd),
                WageTypeNumbers = cycleNumbers,
                JobStatus = PayrunJobStatus.Complete,
                Forecast = scope.PayrunJob.Forecast,
                EvaluationDate = scope.EvaluationDate
            });

        scope.WageTypeCycleCache = new WageTypeCycleCache(
            cycleStart: scope.PayrunJob.CycleStart,
            previousPeriodEnd: previousPeriodEnd,
            wageTypeNumbers: cycleNumbers,
            results: results);
    }

    /// <summary>
    /// Result of a single employee calculation containing the payroll result set,
    /// any retro payrun jobs triggered by scripts, and the retro case value (if any).
    /// </summary>
    private sealed record CalculationResult(
        PayrollResultSet PayrollResult,
        List<RetroPayrunJob> RetroPayrunJobs,
        CaseValue RetroCaseValue);

    /// <summary>
    /// Calculates all wage types and collectors for a single employee within the current
    /// payrun period. Supports execution restarts when a wage type script sets the restart
    /// flag, bounded by <see cref="SystemSpecification.PayrunMaxExecutionCount"/>.
    /// If a Guard wage type calls AbortExecution(), the loop stops immediately;
    /// CollectorEnd and PayrunEmployeeEnd continue normally.
    /// </summary>
    private async Task<CalculationResult> CalculateAsync(PayrunProcessorRegulation processorRegulation,
        Employee employee, ICaseValueProvider caseValueProvider, PayrunEmployeeScope scope,
        PayrunExecutionPhase executionPhase)
    {
        // scope execution
        scope.ExecutionPhase = executionPhase;

        // [culture by priority]: employee > division > tenant > default
        var culture =
            // priority 1: employee culture
            employee.Culture ??
            // priority 2: division culture
            scope.Division.Culture ??
            // priority 3: tenant culture
            Tenant.Culture ??
            // priority 4: deterministic default (independent of server OS locale)
            "en-US";
        scope.PushPayrollCulture(culture);

        // [calendar by priority]: employee > division > tenant
        var calendarName =
            // priority 1: employee calendar
            employee.Calendar ??
            // priority 2: division calendar
            scope.Division.Calendar ??
            // priority 3: tenant calendar
            Tenant.Calendar;
        scope.CalendarName = calendarName;

        // payroll calculator based on the employee culture and calendar
        var employeeCalculator = await CalculatorCache.GetAsync(Tenant.Id, scope.User.Id, scope.PayrollCulture, scope.CalendarName);
        var prevCalculator = scope.Calculator;
        scope.Calculator = employeeCalculator;
        caseValueProvider.PushCalculator(employeeCalculator);

        // payroll results per employee and division
        var payrollResult = new PayrollResultSet
        {
            PayrollId = scope.PayrunJob.PayrollId,
            PayrollName = scope.Payroll.Name,
            PayrunId = Payrun.Id,
            PayrunName = Payrun.Name,
            PayrunJobId = scope.PayrunJob.Id,
            PayrunJobName = scope.PayrunJob.Name,
            EmployeeId = employee.Id,
            EmployeeIdentifier = employee.Identifier,
            DivisionId = scope.Division.Id,
            DivisionName = scope.Division.Name,
            CycleName = scope.PayrunJob.CycleName,
            CycleStart = scope.PayrunJob.CycleStart,
            CycleEnd = scope.PayrunJob.CycleEnd,
            PeriodName = scope.PayrunJob.PeriodName,
            PeriodStart = scope.PayrunJob.PeriodStart,
            PeriodEnd = scope.PayrunJob.PeriodEnd
        };

        // collectors results
        SetupEmployeeCollectors(scope, payrollResult);

        // retro jobs
        var retroPayrunJobs = new List<RetroPayrunJob>();

        // collector start
#if EMPLOYEE_PERFORMANCE
        var perfCalcCollectorStartWatch = System.Diagnostics.Stopwatch.StartNew();
#endif
        foreach (var derivedCollector in scope.DerivedCollectors)
        {
            Log.Trace($"Starting collector {derivedCollector}");
            var collectorResult =
                payrollResult.CollectorResults.First(x => string.Equals(x.CollectorName, derivedCollector.Key));
            var retroJobs = processorRegulation.CollectorStart(scope, derivedCollector, caseValueProvider, payrollResult, collectorResult);
            collectorResult.Value = derivedCollector.First().Result;

            // retro payrun jobs
            PayrunRetroProcessor.AddRetroPayrunJobs(retroPayrunJobs, retroJobs, scope.EvaluationPeriod.Start);
        }
#if EMPLOYEE_PERFORMANCE
        perfCalcCollectorStartWatch.Stop();
        var perfCalcCollectorStartMs = perfCalcCollectorStartWatch.ElapsedMilliseconds;
#endif

        // performance measure
        var stopwatch = LogWatch ? System.Diagnostics.Stopwatch.StartNew() : null;
        var wageTypeStopwatch = stopwatch != null ? System.Diagnostics.Stopwatch.StartNew() : null;
#if EMPLOYEE_PERFORMANCE
        var perfCalcWageTypeWatch = System.Diagnostics.Stopwatch.StartNew();
        var perfCalcWageTypeCount = 0;
        var perfCalcMaxWageTypeMs = 0L;
        var perfCalcSingleWtWatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        // executions (limited by system spec)
        var executionCount = 0;
        bool executionRestart;
        var abortRequested = false;
        do
        {
            // next execution
            executionRestart = false;
            executionCount++;

            // wage types
            foreach (var derivedWageType in scope.DerivedWageTypes)
            {
#if EMPLOYEE_PERFORMANCE
                perfCalcSingleWtWatch.Restart();
#endif
                // use the most derived (leaf) wage type
                Log.Trace($"Payrun processing wage type {derivedWageType.Key} on employee {employee}");

                IPayrollCalculator wageTypeCalculator = null;
                var prevWageTypeCalculator = scope.Calculator;
                try
                {
                    // activate optional wage type calculator
                    var mostDerivedWageType = derivedWageType.First();
                    if (!string.IsNullOrWhiteSpace(mostDerivedWageType.Calendar) &&
                        !string.Equals(mostDerivedWageType.Calendar, scope.CalendarName))
                    {
                        wageTypeCalculator = await CalculatorCache.GetAsync(Tenant.Id, scope.User.Id,
                            scope.PayrollCulture, mostDerivedWageType.Calendar);
                        caseValueProvider.PushCalculator(wageTypeCalculator);
                        scope.Calculator = wageTypeCalculator;
                    }

                    // payrun wage type available
                    if (!processorRegulation.IsWageTypeAvailable(scope, derivedWageType, caseValueProvider))
                    {
                        Log.Trace($"Ignoring employee {employee} on wage type {derivedWageType.Key}");
                        continue;
                    }

                    // calculate result
                    var valueResult = processorRegulation.CalculateWageTypeValue(scope, derivedWageType,
                            payrollResult, caseValueProvider, executionCount);

                    // restart execution
                    if (valueResult != null && valueResult.Item4)
                    {
                        // reset previous results
                        ResetEmployeeResults(scope, payrollResult);
                        // restart the wage type calculation
                        executionRestart = true;
                        // exit wage type loop
                        break;
                    }

                    // abort execution — Guard called AbortExecution()
                    // CollectorEnd and PayrunEmployeeEnd continue normally
                    if (valueResult != null && valueResult.Item5)
                    {
                        abortRequested = true;
                        break;
                    }

                    // wage type result
                    var wageTypeResult = valueResult?.Item1;
                    if (wageTypeResult != null)
                    {
                        payrollResult.WageTypeResults ??= [];
                        payrollResult.WageTypeResults.Add(wageTypeResult);

                        // retro payrun jobs
                        PayrunRetroProcessor.AddRetroPayrunJobs(retroPayrunJobs, valueResult.Item2, scope.EvaluationPeriod.Start);

                        // collector apply
                        foreach (var derivedCollector in scope.DerivedCollectors)
                        {
                            Log.Trace($"Payrun processing collector {derivedCollector.Key}");

                            // disabled collectors
                            if (valueResult.Item3.Contains(derivedCollector.Key))
                            {
                                continue;
                            }

                            // test if collector is available
                            if (PayrunProcessorRegulation.IsCollectorAvailable(derivedWageType, derivedCollector))
                            {
                                // calculate and update collector
                                var collectorResult =
                                    payrollResult.CollectorResults.First(x => string.Equals(x.CollectorName, derivedCollector.Key));
                                var applyResult = processorRegulation.CollectorApply(scope, derivedCollector, caseValueProvider,
                                    wageTypeResult, payrollResult, collectorResult);
                                collectorResult.Value = applyResult.Item1;

                                // retro payrun jobs
                                PayrunRetroProcessor.AddRetroPayrunJobs(retroPayrunJobs, applyResult.Item2, scope.EvaluationPeriod.Start);
                            }
                        }
                    }
                    else
                    {
                        Log.Trace($"Wage type {derivedWageType.Key} without result");
                    }
                }
                finally
                {
                    // restore payroll calculator
                    if (wageTypeCalculator != null)
                    {
                        caseValueProvider.PopCalculator(wageTypeCalculator);
                        scope.Calculator = prevWageTypeCalculator;
                    }
                }

                if (wageTypeStopwatch != null)
                {
                    wageTypeStopwatch.Stop();
                    if (wageTypeStopwatch.Elapsed > Settings.FunctionLogTimeout)
                    {
                        Log.Debug($"{Payrun} calc wage type {derivedWageType.Key:##.#}: {wageTypeStopwatch.ElapsedMilliseconds} ms");
                    }

                    // next wage type
                    wageTypeStopwatch.Restart();
                }

#if EMPLOYEE_PERFORMANCE
                perfCalcSingleWtWatch.Stop();
                perfCalcWageTypeCount++;
                if (perfCalcSingleWtWatch.ElapsedMilliseconds > perfCalcMaxWageTypeMs)
                    perfCalcMaxWageTypeMs = perfCalcSingleWtWatch.ElapsedMilliseconds;
#endif
            }
        } while (executionRestart && !abortRequested && executionCount < SystemSpecification.PayrunMaxExecutionCount);

        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} calc all wage types [{scope.DerivedWageTypes.Count}]: {stopwatch.ElapsedMilliseconds} ms");
        }
#if EMPLOYEE_PERFORMANCE
        perfCalcWageTypeWatch.Stop();
        var perfCalcWageTypeMs = perfCalcWageTypeWatch.ElapsedMilliseconds;
        var perfCalcCollectorEndWatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        // collector end
        foreach (var derivedCollector in scope.DerivedCollectors)
        {
            Log.Trace($"Ending collector {derivedCollector}");
            var collectorResult =
                payrollResult.CollectorResults.First(x => string.Equals(x.CollectorName, derivedCollector.Key));
            var retroJobs = processorRegulation.CollectorEnd(scope, derivedCollector, caseValueProvider, payrollResult, collectorResult);
            collectorResult.Value = derivedCollector.First().Result;

            // retro payrun jobs
            PayrunRetroProcessor.AddRetroPayrunJobs(retroPayrunJobs, retroJobs, scope.EvaluationPeriod.Start);
        }

        // get payrun results
        stopwatch?.Restart();
#if EMPLOYEE_PERFORMANCE
        perfCalcCollectorEndWatch.Stop();
        var perfCalcCollectorEndMs = perfCalcCollectorEndWatch.ElapsedMilliseconds;
        var perfCalcPayrunResultsWatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        // case values as payrun results (with enabled slots)
        payrollResult.PayrunResults.AddRange(
            await processorRegulation.GetCaseValuePayrunResultsAsync(
                payroll: scope.Payroll,
                payrunJob: scope.PayrunJob,
                caseValueProvider: caseValueProvider,
                culture: culture,
                expandCaseSlots: true));

        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} get payrun results [{payrollResult.PayrunResults.Count}]: {stopwatch.ElapsedMilliseconds} ms");
        }
#if EMPLOYEE_PERFORMANCE
        perfCalcPayrunResultsWatch.Stop();
        var perfCalcPayrunResultsMs = perfCalcPayrunResultsWatch.ElapsedMilliseconds;
        var perfCalcPruneWatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        // incremental mode: remove unchanged results
        if (scope.PayrunJob.JobResult == PayrunJobResult.Incremental)
        {
            await ResultPruner.RemoveUnchangedAsync(Tenant.Id, payrollResult, scope.EvaluationDate);
        }

#if EMPLOYEE_PERFORMANCE
        perfCalcPruneWatch.Stop();
        Log.Debug($"{Payrun} CalculateAsync [{executionPhase}] employee perf: " +
            $"collStart={perfCalcCollectorStartMs}ms, wageTypes={perfCalcWageTypeMs}ms " +
            $"({perfCalcWageTypeCount} WTs, maxWT={perfCalcMaxWageTypeMs}ms), " +
            $"collEnd={perfCalcCollectorEndMs}ms, payrunResults={perfCalcPayrunResultsMs}ms, " +
            $"prune={perfCalcPruneWatch.ElapsedMilliseconds}ms");
#endif

        // remove employee calculator
        caseValueProvider.PopCalculator(employeeCalculator);
        scope.Calculator = prevCalculator;

        // restore culture
        scope.PopPayrollCulture(culture);

        // set results creation date
        payrollResult.SetResultDate(scope.EvaluationDate);

        // set denormalized context for direct index seeks
        payrollResult.SetDenormalizedContext(Tenant.Id, employee.Id, scope.Division.Id,
            scope.PayrunJob.Id, scope.PayrunJob.Forecast, scope.PayrunJob.ParentJobId);

        return new CalculationResult(payrollResult, retroPayrunJobs, caseValueProvider.RetroCaseValue);
    }

    /// <summary>
    /// Resets all in-progress calculation results for the current employee.
    /// Called when a wage type script requests an execution restart.
    /// </summary>
    private static void ResetEmployeeResults(PayrunEmployeeScope scope, PayrollResultSet payrollResult)
    {
        // reset collectors
        SetupEmployeeCollectors(scope, payrollResult);

        // reset wage type results
        payrollResult.WageTypeResults?.Clear();

        // reset payrun results
        payrollResult.PayrunResults?.Clear();
    }

    /// <summary>
    /// Clones the shared source collectors into fresh per-employee instances and initialises
    /// the corresponding <see cref="CollectorResultSet"/> entries on the payroll result.
    /// Called once at the start of each employee calculation and again on execution restart.
    /// </summary>
    private static void SetupEmployeeCollectors(PayrunEmployeeScope scope, PayrollResultSet payrollResult)
    {
        // ensure clean collector results
        if (payrollResult.CollectorResults != null)
        {
            payrollResult.CollectorResults.Clear();
        }
        else
        {
            payrollResult.CollectorResults = [];
        }

        // clone collectors from source for this employee (fresh values instead of Reset)
        scope.DerivedCollectors = scope.SharedContext.SourceDerivedCollectors
            .SelectMany(g => g.Select(c => new { g.Key, Collector = c.Clone() }))
            .ToLookup(x => x.Key, x => x.Collector);

        foreach (var derivedCollector in scope.DerivedCollectors)
        {
            // add single result
            var resultCollector = derivedCollector.First();
            payrollResult.CollectorResults.Add(new()
            {
                CollectorId = resultCollector.Id,
                CollectorName = resultCollector.Name,
                CollectorNameLocalizations = resultCollector.NameLocalizations,
                CollectMode = resultCollector.CollectMode,
                Negated = resultCollector.Negated,
                ValueType = resultCollector.ValueType,
                Culture = resultCollector.Culture ?? scope.PayrollCulture,
                Start = scope.EvaluationPeriod.Start,
                End = scope.EvaluationPeriod.End,
                Attributes = new(),
                CustomResults = []
            });
        }
    }
}
