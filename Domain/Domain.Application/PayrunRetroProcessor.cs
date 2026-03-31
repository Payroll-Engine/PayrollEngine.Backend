//#define RETRO_PERFORMANCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Handles retroactive payrun processing: determines the retro evaluation date,
/// resolves the effective retro date from case values and script-triggered retro jobs,
/// iterates retro periods, and cleans up retro jobs on failure.
/// <para>
/// Retro processing is one level deep: retro jobs are created with
/// <see cref="RetroPayMode.None"/> so that the recursive process call
/// does not spawn further retro jobs.
/// </para>
/// </summary>
internal sealed class PayrunRetroProcessor
{
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }
    private PayrunProcessorSettings Settings { get; }
    private bool IsPreview { get; }

    /// <summary>
    /// Initializes a new <see cref="PayrunRetroProcessor"/>.
    /// </summary>
    /// <param name="tenant">The owning tenant.</param>
    /// <param name="payrun">The payrun definition.</param>
    /// <param name="settings">Processor settings including repositories and configuration.</param>
    /// <param name="isPreview">True when running in preview mode (no DB writes).</param>
    internal PayrunRetroProcessor(
        Tenant tenant,
        Payrun payrun,
        PayrunProcessorSettings settings,
        bool isPreview)
    {
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        IsPreview = isPreview;
    }

    /// <summary>
    /// Determines the retro evaluation date for the current invocation by finding the most
    /// recent completed full payrun job for the same payrun and period. Returns <c>null</c>
    /// when retro pay is disabled or no previous job exists.
    /// </summary>
    /// <param name="jobInvocation">The job invocation containing retro pay mode and period info.</param>
    /// <returns>The retro evaluation date, or <c>null</c> if no retro processing is needed.</returns>
    internal async Task<DateTime?> GetRetroDateAsync(PayrunJobInvocation jobInvocation)
    {
        if (jobInvocation.RetroPayMode == RetroPayMode.None)
        {
            // no retro pay support
            return null;
        }

#if RETRO_PERFORMANCE
        var perfRetroWatch = System.Diagnostics.Stopwatch.StartNew();
#endif
        // Build filter: same payrun, full jobs only, strictly before current period
        // Legal vs. forecast: legal jobs have null/empty Forecast; forecast jobs match by name
        var forecastFilter = string.IsNullOrWhiteSpace(jobInvocation.Forecast)
            ? $" and ({nameof(PayrunJob.Forecast)} eq null or {nameof(PayrunJob.Forecast)} eq '')"
            : $" and {nameof(PayrunJob.Forecast)} eq '{jobInvocation.Forecast}'";

        var filter =
            $"{nameof(PayrunJob.PayrunId)} eq {Payrun.Id}" +
            $" and {nameof(PayrunJob.JobResult)} eq '{Enum.GetName(typeof(PayrunJobResult), PayrunJobResult.Full)}'" +
            $" and {nameof(PayrunJob.PeriodStart)} lt '{jobInvocation.PeriodStart:O}'" +
            forecastFilter;

        // Ask the DB for the single most-recent applicable job — avoids loading all rows
        var query = new Query
        {
            Filter = filter,
            OrderBy = $"{nameof(PayrunJob.EvaluationDate)} desc, {nameof(PayrunJob.Created)} desc",
            Top = 1
        };

        var previousJob = (await Settings.PayrunJobRepository.QueryAsync(Settings.DbContext, Tenant.Id, query))
            .FirstOrDefault();

        if (previousJob == null)
        {
            return null;
        }

#if RETRO_PERFORMANCE
        perfRetroWatch.Stop();
        Log.Debug($"GetRetroDateAsync: {perfRetroWatch.ElapsedMilliseconds} ms, " +
            $"retro date {previousJob.EvaluationDate:yyyy-MM-dd}");
#endif
        return previousJob.EvaluationDate.ToUtc();
    }

    /// <summary>
    /// Resolves the effective retro date from case value retro dates and
    /// script-triggered retro jobs, applying <see cref="Payrun.RetroBackCycles"/> restrictions.
    /// Returns <c>null</c> if no retro processing is needed.
    /// </summary>
    /// <param name="caseValueRetroDate">The retro date from case values; <c>null</c> if none.</param>
    /// <param name="scriptRetroJobs">Retro jobs triggered by collector/wage type scripts.</param>
    /// <param name="currentJob">The current payrun job providing the current cycle boundary.</param>
    /// <param name="calculator">The payroll calculator used to navigate cycle boundaries.</param>
    /// <returns>The effective retro date after applying restrictions, or <c>null</c>.</returns>
    internal DateTime? ResolveEffectiveRetroDate(
        DateTime? caseValueRetroDate,
        List<RetroPayrunJob> scriptRetroJobs,
        PayrunJob currentJob,
        IPayrollCalculator calculator)
    {
        var retroDate = caseValueRetroDate;

        // target retro date from script-triggered retro jobs
        if (scriptRetroJobs.Any())
        {
            foreach (var payrunRetroJob in scriptRetroJobs)
            {
                if (retroDate == null || payrunRetroJob.ScheduleDate < retroDate.Value)
                {
                    // retro date by script (manual)
                    retroDate = payrunRetroJob.ScheduleDate;
                }
            }
        }

        // retro cycle boundary restriction
        // -1 = unlimited: no clamping needed
        if (retroDate.HasValue && Payrun.RetroBackCycles >= 0)
        {
            // walk back RetroBackCycles cycles from the current cycle start
            var cycleStart = currentJob.CycleStart;
            for (var i = 0; i < Payrun.RetroBackCycles; i++)
            {
                // one day before this cycle's start lands inside the previous cycle
                cycleStart = calculator.GetPayrunCycle(cycleStart.AddDays(-1)).Start;
            }

            // clamp: retro must not reach before the earliest allowed cycle start
            if (retroDate < cycleStart)
            {
                retroDate = cycleStart;
            }
        }

        return retroDate;
    }

    /// <summary>
    /// Executes retro payrun jobs for all periods between <paramref name="retroDate"/> and the
    /// current period. Creates a new <see cref="PayrunProcessor"/> per retro period (one level recursive).
    /// </summary>
    /// <param name="retroDate">The effective retro start date.</param>
    /// <param name="employee">The employee being processed.</param>
    /// <param name="scope">The per-employee execution scope.</param>
    /// <param name="employeeCaseValues">The employee's case value cache for reuse.</param>
    /// <param name="scriptRetroJobs">Retro jobs triggered by scripts.</param>
    /// <returns>The list of completed retro payrun jobs.</returns>
    /// <exception cref="PayrunException">Thrown when the retro period limit is exceeded.</exception>
    internal async Task<List<PayrunJob>> ProcessRetroPeriodsAsync(
        DateTime retroDate,
        Employee employee,
        PayrunEmployeeScope scope,
        CaseValueCache employeeCaseValues,
        List<RetroPayrunJob> scriptRetroJobs)
    {
        var retroJobs = new List<PayrunJob>();

#if RETRO_PERFORMANCE
        var perfRetroTotalWatch = System.Diagnostics.Stopwatch.StartNew();
#endif
        // repeat until previous period
        var retroPeriod = scope.Calculator.GetPayrunPeriod(retroDate);
        var maxRetroPeriods = Settings.MaxRetroPayrunPeriods;
        var retroPeriodCount = 0;
        while (retroPeriod != null && retroPeriod.Start < scope.EvaluationPeriod.Start)
        {
            // safety guard: prevent runaway retro calculations
            if (maxRetroPeriods > 0 && ++retroPeriodCount > maxRetroPeriods)
            {
                throw new PayrunException(
                    $"Retro period limit exceeded ({maxRetroPeriods}). " +
                    $"Retro date {retroDate} for employee {employee.Identifier} is too far in the past. " +
                    $"Adjust the retro date or increase MaxRetroPayrunPeriods in appsettings.");
            }

            // retro payrun processor (one level recursive)
            var retroProcessor = new PayrunProcessor(Tenant, Payrun, Settings);

            // retro payrun job invocation
            var retroJobInvocation = new PayrunJobInvocation
            {
                ParentJobId = scope.PayrunJob.Id,
                PayrunName = Payrun.Name,
                UserIdentifier = scope.User.Identifier,
                Name = scope.PayrunJob.Name,
                Owner = scope.PayrunJob.Owner,
                Forecast = scope.PayrunJob.Forecast,
                // retro jobs
                RetroJobs = scriptRetroJobs,
                // ensure no recursive retro pay calculation
                RetroPayMode = RetroPayMode.None,
                // incremental results only for retro pay run jobs
                JobResult = PayrunJobResult.Incremental,
                // retro payrun job status is complete
                JobStatus = PayrunJobStatus.Complete,
                PeriodStart = retroPeriod.Start,
                EvaluationDate = scope.EvaluationDate,
                Reason = scope.PayrunJob.CreatedReason,
                StoreEmptyResults = scope.StoreEmptyResults,
                // current employee only
                EmployeeIdentifiers = [employee.Identifier],
                // consider runtime attribute changes
                Attributes = scope.PayrunJob.Attributes,
            };

            // setup with shared caches
            var payrunSetup = new PayrunProcessor.PayrunSetup
            {
                Payroll = scope.Payroll,
                Division = scope.Division,
                Employees = [employee],
                GlobalCaseValues = scope.GlobalCaseValues,
                NationalCaseValues = scope.NationalCaseValues,
                CompanyCaseValues = scope.CompanyCaseValues,
                EmployeeCaseValues = employeeCaseValues
            };

            // process retro payrun job (one level recursive)
#if RETRO_PERFORMANCE
            var perfRetroPeriodWatch = System.Diagnostics.Stopwatch.StartNew();
#endif
            var retroJob = await retroProcessor.Process(retroJobInvocation, payrunSetup);
            retroJobs.Add(retroJob);

            // complete retro job
            await CompleteRetroJobAsync(retroJob);
#if RETRO_PERFORMANCE
            perfRetroPeriodWatch.Stop();
            Log.Information($"{Payrun} retro period {retroPeriod.Start:yyyy-MM} " +
                $"employee {employee.Identifier}: {perfRetroPeriodWatch.ElapsedMilliseconds} ms");
#endif

            // prepare period for the following retro payrun job
            retroPeriod = retroPeriod.GetPayrollPeriod(retroPeriod.Start, 1);
        }

#if RETRO_PERFORMANCE
        perfRetroTotalWatch.Stop();
        if (retroJobs.Count > 0)
        {
            Log.Information($"{Payrun} retro processing employee {employee.Identifier}: " +
                $"{retroJobs.Count} period(s), total {perfRetroTotalWatch.ElapsedMilliseconds} ms, " +
                $"avg {perfRetroTotalWatch.ElapsedMilliseconds / retroJobs.Count} ms/period");
        }
#endif

        return retroJobs;
    }

    /// <summary>
    /// Cleans up retro jobs on failure: attempts to delete each retro job from the database.
    /// If deletion fails, marks the job with an error message.
    /// </summary>
    /// <param name="retroJobs">The retro jobs to clean up.</param>
    /// <param name="employee">The employee whose retro jobs failed.</param>
    /// <param name="parentJob">The parent payrun job for error message context.</param>
    internal async Task CleanupRetroJobsAsync(
        List<PayrunJob> retroJobs,
        Employee employee,
        PayrunJob parentJob)
    {
        foreach (var retroJob in retroJobs)
        {
            var deleted = false;
            try
            {
                Log.Trace($"Retro payrun job {retroJob.Name}: cleanup of employee {employee.Identifier} for period {retroJob.PeriodStart}");

                // cleanup retro payrun job
                await Settings.PayrunJobRepository.DeleteAsync(Settings.DbContext, Tenant.Id, retroJob.Id);
                deleted = true;
                Log.Trace($"Retro payrun job {retroJob.Name}: deleted successfully");
            }
            catch (Exception exception)
            {
                Log.Error(exception.GetBaseMessage(), exception);
            }

            // Only attempt to update the job message if delete failed,
            // i.e. the entity may still exist in the database.
            // After a successful delete the entity is gone; calling UpdateJobAsync
            // would always fail with a "not found" error.
            if (!deleted)
            {
                try
                {
                    retroJob.ErrorMessage =
                        $"Retro Payrun job {retroJob.Name}: cleanup failed for employee {employee.Identifier} " +
                        $"for period {retroJob.PeriodStart} in parent Payrun {parentJob.Name}";
                    await UpdateJobAsync(retroJob);
                }
                catch (Exception exception)
                {
                    // entity may have been deleted by another process; log and move on
                    Log.Error(exception.GetBaseMessage(), exception);
                }
            }
        }
    }

    /// <summary>
    /// Validates and appends retro jobs to the list. Each retro job's schedule date
    /// must lie before <paramref name="evaluationPeriodStart"/>.
    /// </summary>
    /// <param name="payrunJobs">The target list to append to.</param>
    /// <param name="retroJobs">The retro jobs to validate and append.</param>
    /// <param name="evaluationPeriodStart">The current evaluation period start for validation.</param>
    /// <exception cref="PayrollException">Thrown when a retro job's schedule date is after the evaluation period start.</exception>
    internal static void AddRetroPayrunJobs(List<RetroPayrunJob> payrunJobs, List<RetroPayrunJob> retroJobs, DateTime evaluationPeriodStart)
    {
        if (retroJobs != null && retroJobs.Any())
        {
            foreach (var payrunJob in retroJobs)
            {
                if (payrunJob.ScheduleDate > evaluationPeriodStart)
                {
                    throw new PayrollException(
                        $"Retro job schedule date {payrunJob.ScheduleDate} must be before {evaluationPeriodStart}.");
                }
                payrunJobs.Add(payrunJob);
            }
        }
    }

    /// <summary>
    /// Marks a retro payrun job as successfully completed and persists the final state.
    /// </summary>
    private async Task CompleteRetroJobAsync(PayrunJob payrunJob)
    {
        // setup
        payrunJob.JobStatus = PayrunJobStatus.Complete;
        payrunJob.JobEnd = Date.Now;
        var duration = payrunJob.JobEnd.Value - payrunJob.JobStart;
        payrunJob.Message = $"Completed retro payrun calculation successfully in {duration.ToReadableString()}.";
        Log.Debug(payrunJob.Message);

        // preview mode: keep completion state in-memory only
        if (!IsPreview)
        {
            await Settings.PayrunJobRepository.UpdateAsync(Settings.DbContext, Tenant.Id, payrunJob);
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
}
