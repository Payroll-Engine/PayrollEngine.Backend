using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Loads and initialises all regulation-related objects for a payrun job:
/// derived regulations, case field and lookup providers, case value caches,
/// derived collectors and wage types.
/// </summary>
internal sealed class PayrunRegulationInitializer
{
    private PayrunProcessorSettings Settings { get; }
    private FunctionHost FunctionHost { get; }
    private IResultProvider ResultProvider { get; }
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }
    private bool LogWatch { get; }

    /// <summary>
    /// Initializes a new <see cref="PayrunRegulationInitializer"/>.
    /// </summary>
    internal PayrunRegulationInitializer(
        PayrunProcessorSettings settings,
        FunctionHost functionHost,
        IResultProvider resultProvider,
        Tenant tenant,
        Payrun payrun,
        bool logWatch)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        FunctionHost = functionHost ?? throw new ArgumentNullException(nameof(functionHost));
        ResultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
        LogWatch = logWatch;
    }

    /// <summary>
    /// Loads derived regulations, sets up case field and regulation lookup providers,
    /// initialises case value caches (global, national, company), and loads derived
    /// collectors and wage types.
    /// </summary>
    /// <param name="context">The payrun context to populate with regulations and providers.</param>
    /// <param name="setup">Preloaded case value caches; null members are created fresh.</param>
    /// <param name="processorRepositories">Repository facade for loading derived regulations.</param>
    /// <returns>The <see cref="PayrunProcessorRegulation"/> needed for employee processing.</returns>
    internal async Task<PayrunProcessorRegulation> InitializeAsync(
        PayrunContext context, PayrunProcessor.PayrunSetup setup,
        PayrunProcessorRepositories processorRepositories)
    {
        // derived regulations
        context.DerivedRegulations = await processorRepositories.LoadDerivedRegulationsAsync(
            payrollId: context.Payroll.Id,
            regulationDate: context.PayrunJob.PeriodEnd,
            evaluationDate: context.EvaluationDate);

        // case field provider
        ClusterSet caseFieldClusterSet = null;
        if (!string.IsNullOrWhiteSpace(context.Payroll.ClusterSetCaseField))
        {
            caseFieldClusterSet = context.Payroll.ClusterSets.FirstOrDefault(x => string.Equals(context.Payroll.ClusterSetCaseField, x.Name));
        }
        context.CaseFieldProvider = new CaseFieldProvider(
            new CaseFieldProxyRepository(Settings.PayrollRepository, Tenant.Id, Payrun.PayrollId,
                context.PayrunJob.PeriodEnd, context.EvaluationDate, caseFieldClusterSet));

        // case value caches (global, national, company)
        var cacheFactory = new CaseValueCacheFactory(
            Settings.DbContext, context.Division.Id, context.EvaluationDate, context.PayrunJob.Forecast);
        context.GlobalCaseValues = setup.GlobalCaseValues ??
            cacheFactory.Create(Settings.GlobalCaseValueRepository, Tenant.Id);
        context.NationalCaseValues = setup.NationalCaseValues ??
            cacheFactory.Create(Settings.NationalCaseValueRepository, Tenant.Id);
        context.CompanyCaseValues = setup.CompanyCaseValues ??
            cacheFactory.Create(Settings.CompanyCaseValueRepository, Tenant.Id);

        // processor regulation
        var processorRegulation = new PayrunProcessorRegulation(
            functionHost: FunctionHost,
            settings: Settings,
            resultProvider: ResultProvider,
            tenant: Tenant,
            payroll: context.Payroll,
            payrun: Payrun);

        // regulation lookup provider
        context.RegulationLookupProvider = new RegulationLookupProvider(
            dbContext: Settings.DbContext,
            payrollRepository: Settings.PayrollRepository,
            payrollQuery: new()
            {
                TenantId = Tenant.Id,
                PayrollId = context.Payroll.Id,
                RegulationDate = context.PayrunJob.PeriodEnd,
                EvaluationDate = context.PayrunJob.EvaluationDate
            },
            regulationRepository: Settings.RegulationRepository,
            lookupSetRepository: Settings.RegulationLookupSetRepository);

        // derived collectors (optional) - loaded once as clone source per employee
        context.SourceDerivedCollectors = await GetDerivedCollectors(
            payroll: context.Payroll,
            payrunJob: context.PayrunJob,
            processorRegulation: processorRegulation);

        // derived wage types
        context.DerivedWageTypes = await GetDerivedWageTypes(
            payroll: context.Payroll,
            payrunJob: context.PayrunJob,
            processorRegulation: processorRegulation);

        return processorRegulation;
    }

    /// <summary>
    /// Loads the active derived collectors for the given payrun job, respecting cluster sets
    /// and the retro-job collector cluster override.
    /// </summary>
    private async Task<ILookup<string, DerivedCollector>> GetDerivedCollectors(Payroll payroll,
        PayrunJob payrunJob, PayrunProcessorRegulation processorRegulation)
    {
        // performance measure
        var stopwatch = LogWatch ? System.Diagnostics.Stopwatch.StartNew() : null;

        // context derived collectors grouped by collector name
        var collectorCluster = payroll.ClusterSetCollector;
        if (payrunJob.IsRetroJob && !string.IsNullOrWhiteSpace(payroll.ClusterSetCollectorRetro))
        {
            // retro job override
            collectorCluster = payroll.ClusterSetCollectorRetro;
        }
        var clusterSetCollector = payroll.ClusterSets?.FirstOrDefault(x => string.Equals(collectorCluster, x.Name));
        var derivedCollectors = await processorRegulation.GetDerivedCollectorsAsync(payrunJob, clusterSetCollector);

        Log.Trace($"{Payrun} with {derivedCollectors.Count} collectors");

        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} load derived collectors [{derivedCollectors.Count}]: {stopwatch.ElapsedMilliseconds} ms");
        }

        return derivedCollectors;
    }

    /// <summary>
    /// Loads the active derived wage types for the given payrun job, respecting cluster sets
    /// and the retro-job wage type cluster override.
    /// </summary>
    private async Task<ILookup<decimal, DerivedWageType>> GetDerivedWageTypes(Payroll payroll, PayrunJob payrunJob,
        PayrunProcessorRegulation processorRegulation)
    {
        // performance measure
        var stopwatch = LogWatch ? System.Diagnostics.Stopwatch.StartNew() : null;

        // context derived wage types grouped by wage type identifier
        var wageTypeCluster = payroll.ClusterSetWageType;
        if (payrunJob.IsRetroJob && !string.IsNullOrWhiteSpace(payroll.ClusterSetWageTypeRetro))
        {
            // retro job override
            wageTypeCluster = payroll.ClusterSetWageTypeRetro;
        }
        var clusterSetWageType = payroll.ClusterSets?.FirstOrDefault(x => string.Equals(wageTypeCluster, x.Name));
        var derivedWageTypes = await processorRegulation.GetDerivedWageTypesAsync(payrunJob, clusterSetWageType);

        Log.Trace($"Payrun with {derivedWageTypes.Count} wage types");
        if (stopwatch != null)
        {
            stopwatch.Stop();
            Log.Debug($"{Payrun} load derived wage types [{derivedWageTypes.Count}]: {stopwatch.ElapsedMilliseconds} ms");
        }

        return derivedWageTypes;
    }
}
