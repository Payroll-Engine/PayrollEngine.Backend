using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Builds the <see cref="PayrunContext"/> for a payrun job invocation:
/// resolves user, payroll, division, culture, calendar, and calculator;
/// creates or loads the payrun job entity and resolves parent jobs for
/// retro invocations.
/// </summary>
internal sealed class PayrunContextBuilder
{
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }
    private PayrollCalculatorCache CalculatorCache { get; }
    private PayrunRetroProcessor RetroProcessor { get; }

    /// <summary>
    /// Initializes a new <see cref="PayrunContextBuilder"/>.
    /// </summary>
    internal PayrunContextBuilder(
        Tenant tenant,
        Payrun payrun,
        PayrollCalculatorCache calculatorCache,
        PayrunRetroProcessor retroProcessor)
    {
       // Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
        CalculatorCache = calculatorCache ?? throw new ArgumentNullException(nameof(calculatorCache));
        RetroProcessor = retroProcessor ?? throw new ArgumentNullException(nameof(retroProcessor));
    }

    /// <summary>
    /// Builds the initial <see cref="PayrunContext"/> including user, payroll, division,
    /// culture, calendar, and calculator. Uses preloaded values from <paramref name="setup"/>
    /// where available; loads missing values from the database.
    /// </summary>
    /// <param name="jobInvocation">Invocation parameters for user, payrun, and retro date resolution.</param>
    /// <param name="setup">Preloaded context objects; null members are loaded lazily.</param>
    /// <param name="processorRepositories">Repository facade for loading entities.</param>
    /// <returns>A fully initialised <see cref="PayrunContext"/> (without job, regulations, or employees).</returns>
    internal async Task<PayrunContext> BuildContextAsync(
        PayrunJobInvocation jobInvocation, PayrunProcessor.PayrunSetup setup,
        PayrunProcessorRepositories processorRepositories)
    {
        // resolve user by identifier
        var userId = await processorRepositories.ResolveUserIdAsync(jobInvocation.UserIdentifier);

        // resolve payroll from payrun (Payrun is already loaded as class member)
        var payrollId = Payrun.PayrollId;

        var context = new PayrunContext
        {
            User = await processorRepositories.LoadUserAsync(userId),
            Payroll = setup.Payroll ?? await processorRepositories.LoadPayrollAsync(payrollId),
            RetroDate = await RetroProcessor.GetRetroDateAsync(jobInvocation),
            StoreEmptyResults = jobInvocation.StoreEmptyResults
        };

        // context division
        context.Division = setup.Division ?? await processorRepositories.LoadDivisionAsync(context.Payroll.DivisionId);

        // [culture by priority]: division > tenant > default
        var cultureName =
            context.Division.Culture ??
            Tenant.Culture ??
            // deterministic default (independent of server OS locale)
            "en-US";
        context.SetPayrollCulture(cultureName);

        // calendar
        context.CalendarName = context.Division.Calendar ?? Tenant.Calendar;

        // calculator
        context.Calculator = await CalculatorCache.GetAsync(
            tenantId: Tenant.Id,
            userId: context.User.Id,
            culture: context.PayrollCulture,
            calendarName: context.CalendarName);

        return context;
    }

    /// <summary>
    /// Creates a new <see cref="PayrunJob"/> or loads and updates an existing pre-created job
    /// (async controller scenario). Also resolves the parent job for retro invocations and
    /// sets evaluation date and period on the context.
    /// </summary>
    /// <param name="jobInvocation">Invocation parameters including the optional pre-created job id.</param>
    /// <param name="context">The payrun context whose <see cref="PayrunContext.PayrunJob"/> is populated.</param>
    /// <param name="processorRepositories">Repository facade for loading the job entity.</param>
    internal async Task CreateOrLoadJobAsync(
        PayrunJobInvocation jobInvocation, PayrunContext context,
        PayrunProcessorRepositories processorRepositories)
    {
        var payrollId = context.Payroll.Id;

        // Check if job was pre-created by async controller
        if (jobInvocation.PayrunJobId > 0)
        {
            // Job already created by async controller - load and update it
            context.PayrunJob = await processorRepositories.LoadPayrunJobAsync(jobInvocation.PayrunJobId)
                ?? throw new PayrunException($"Payrun job with id {jobInvocation.PayrunJobId} not found");

            // Update job with calculated period/cycle info from calculator
            PayrunJobFactory.UpdatePayrunJob(
                payrunJob: context.PayrunJob,
                jobInvocation: jobInvocation,
                divisionId: context.Division.Id,
                payrollId: payrollId,
                payrollCalculator: context.Calculator);
        }
        else
        {
            // Create new job (sync mode or retro jobs)
            context.PayrunJob = PayrunJobFactory.CreatePayrunJob(
                jobInvocation: jobInvocation,
                payrunId: Payrun.Id,
                userId: context.User.Id,
                divisionId: context.Division.Id,
                payrollId: payrollId,
                payrollCalculator: context.Calculator);
        }

        // parent job for retro invocations
        if (context.PayrunJob.ParentJobId.HasValue)
        {
            context.ParentPayrunJob = await processorRepositories.LoadPayrunJobAsync(context.PayrunJob.ParentJobId.Value)
                ?? throw new PayrunException($"Parent payrun job with id {context.PayrunJob.ParentJobId.Value} not found");
            context.RetroPayrunJobs = jobInvocation.RetroJobs;
        }

        // update invocation (only needed for new jobs)
        if (jobInvocation.PayrunJobId == 0)
        {
            jobInvocation.PayrunJobId = context.PayrunJob.Id;
        }

        // context dates
        context.EvaluationDate = context.PayrunJob.EvaluationDate;
        context.EvaluationPeriod = context.PayrunJob.GetEvaluationPeriod();
    }
}
