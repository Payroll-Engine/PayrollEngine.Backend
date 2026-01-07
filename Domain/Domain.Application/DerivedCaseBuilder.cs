using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Application;

public class DerivedCaseBuilder : DerivedCaseTool
{
    /// <summary>
    /// Constructor for global cases
    /// </summary>
    public DerivedCaseBuilder(
        IGlobalCaseValueRepository globalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, settings)
    {
    }

    /// <summary>
    /// Constructor for national cases
    /// </summary>
    public DerivedCaseBuilder(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, nationalCaseValueRepository, settings)
    {
    }

    /// <summary>
    /// Constructor for company cases
    /// </summary>
    public DerivedCaseBuilder(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, nationalCaseValueRepository, companyCaseValueRepository, settings)
    {
    }

    /// <summary>
    /// Constructor for employee cases
    /// </summary>
    public DerivedCaseBuilder(Employee employee,
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        IEmployeeCaseValueRepository employeeCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(employee, globalCaseValueRepository, nationalCaseValueRepository, companyCaseValueRepository,
            employeeCaseValueRepository, settings)
    {
    }

    public async Task<CaseSet> BuildGlobalCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, string culture) =>
        await BuildCaseAsync(CaseType.Global, caseName, caseChangeSetup, culture);

    public async Task<CaseSet> BuildNationalCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, string culture) =>
        await BuildCaseAsync(CaseType.National, caseName, caseChangeSetup, culture);

    public async Task<CaseSet> BuildCompanyCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, string culture) =>
        await BuildCaseAsync(CaseType.Company, caseName, caseChangeSetup, culture);

    public async Task<CaseSet> BuildEmployeeCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, string culture) =>
        await BuildCaseAsync(CaseType.Employee, caseName, caseChangeSetup, culture);

    private async Task<CaseSet> BuildCaseAsync(CaseType caseType, string caseName,
        CaseChangeSetup caseChangeSetup, string culture)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }
        if (caseChangeSetup == null)
        {
            throw new ArgumentNullException(nameof(caseChangeSetup));
        }

        // case (derived)
        var cases = (await PayrollRepository.GetDerivedCasesAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            caseType: caseType,
            caseNames: [caseName],
            clusterSet: ClusterSet,
            overrideType: OverrideType.Active)).Cast<Case>().ToList();
        if (!cases.Any())
        {
            Log.Trace($"Missing case {caseName} in payroll with id {Payroll.Id}");
            return new();
        }

        // derived case from the most derived one
        var caseSlot = caseChangeSetup.Case?.CaseSlot;
        var caseSet = await GetDerivedCaseSetAsync(cases, caseSlot, caseChangeSetup, culture, true);

        // resolve case: start of recursion
        await BuildCaseAsync(cases, caseSet, caseChangeSetup, culture);

        return caseSet;
    }

    // entry point to resolve recursive
    private async Task<bool> BuildCaseAsync(IList<Case> cases, CaseSet caseSet,
        CaseChangeSetup caseChangeSetup, string culture)
    {
        var build = CaseBuild(cases, caseSet);
        if (!build)
        {
            Log.Trace($"Build failed for case {caseSet.Name}");
            return false;
        }

        // case relations (active only)
        var relations = (await PayrollRepository.GetDerivedCaseRelationsAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            sourceCaseName: caseSet.Name,
            clusterSet: ClusterSet,
            overrideType: OverrideType.Active)).ToList();
        if (!relations.Any())
        {
            Log.Trace($"No related cases available for case {caseSet.Name}");
            return true;
        }

        // setup related cases
        caseSet.RelatedCases = [];

        // group by relations (same source to target relations)
        var targetRelations = relations.GroupBy(x => new { x.SourceCaseName, x.SourceCaseSlot, x.TargetCaseName, x.TargetCaseSlot });
        foreach (var targetRelation in targetRelations)
        {
            // source case filter
            var sourceCaseSlot = targetRelation.Key.SourceCaseSlot;
            if (!string.Equals(sourceCaseSlot, caseSet.CaseSlot))
            {
                continue;
            }

            // target case (derived)
            var targetCases = (await PayrollRepository.GetDerivedCasesAsync(Settings.DbContext,
                new()
                {
                    TenantId = Tenant.Id,
                    PayrollId = Payroll.Id,
                    RegulationDate = RegulationDate,
                    EvaluationDate = EvaluationDate
                },
                caseNames: [targetRelation.Key.TargetCaseName],
                clusterSet: ClusterSet,
                overrideType: OverrideType.Active)).Cast<Case>().ToList();
            if (!targetCases.Any())
            {
                throw new PayrollException($"Unknown related case with name {targetRelation.Key.TargetCaseName} in derived case {caseSet.Name}.");
            }
            // target derived case on the most derived one
            var targetCaseSet = await GetDerivedCaseSetAsync(targetCases, targetRelation.Key.TargetCaseSlot, caseChangeSetup, culture, true);

            // build case relation
            if (!CaseRelationBuild(targetRelation.ToList(), caseSet, targetCaseSet))
            {
                Log.Trace($"Ignoring case relation from {caseSet.Name} to {targetCaseSet.Name}");
                continue;
            }

            // case slots
            var relation = targetRelation.First();
            var caseSlot = relation.TargetCaseSlot;
            if (!string.IsNullOrWhiteSpace(caseSlot))
            {
                // slot
                var slot = targetCaseSet.Slots.FirstOrDefault(x => string.Equals(x.Name, caseSlot));
                var caseSlotLocalizations = slot != null ? slot.NameLocalizations : relation.TargetCaseSlotLocalizations;

                // target case
                targetCaseSet.CaseSlot = caseSlot;
                targetCaseSet.CaseSlotLocalizations = caseSlotLocalizations;

                // target case fields
                foreach (var field in targetCaseSet.Fields)
                {
                    field.CaseSlot = caseSlot;
                    field.CaseSlotLocalizations = caseSlotLocalizations;
                }
            }

            // process related case (recursive)
            if (await BuildCaseAsync(targetCases, targetCaseSet, caseChangeSetup, culture))
            {
                // add related case (ignore invalid case)
                caseSet.RelatedCases.Add(targetCaseSet);
                Log.Trace($"Added related case {targetCaseSet.Name} to case {caseSet.Name}");
            }
        }

        return true;
    }

    private bool CaseBuild(IEnumerable<Case> cases, CaseSet caseSet)
    {
        var lookupProvider = NewRegulationLookupProvider();

        // case build expression
        var build = true;

        var settings = new CaseChangeRuntimeSettings
        {
            DbContext = Settings.DbContext,
            UserCulture = UserCulture,
            FunctionHost = FunctionHost,
            Tenant = Tenant,
            User = User,
            Payroll = Payroll,
            CaseProvider = CaseProvider,
            CaseValueProvider = CaseValueProvider,
            RegulationLookupProvider = lookupProvider,
            DivisionRepository = DivisionRepository,
            EmployeeRepository = EmployeeRepository,
            CalendarRepository = CalendarRepository,
            PayrollCalculatorProvider = PayrollCalculatorProvider,
            WebhookDispatchService = WebhookDispatchService,
            Case = caseSet
        };

        foreach (var buildScripts in cases.GetDerivedExpressionObjects(x => x.BuildScript))
        {
            var caseBuild = new CaseScriptController().CaseBuild(buildScripts, settings);
            if (caseBuild.HasValue)
            {
                build = caseBuild.Value;
                break;
            }
        }

        // remove inactive case field
        if (caseSet.Fields != null)
        {
            caseSet.Fields = caseSet.Fields.Where(x => x.Status == ObjectStatus.Active).ToList();
        }

        return build;
    }

    private bool CaseRelationBuild(IEnumerable<CaseRelation> derivedCaseRelation, CaseSet sourceCaseSet, CaseSet targetCaseSet)
    {
        var lookupProvider = NewRegulationLookupProvider();

        var settings = new CaseRelationRuntimeSettings
        {
            DbContext = Settings.DbContext,
            UserCulture = UserCulture,
            FunctionHost = FunctionHost,
            Tenant = Tenant,
            User = User,
            Payroll = Payroll,
            CaseValueProvider = CaseValueProvider,
            RegulationLookupProvider = lookupProvider,
            DivisionRepository = DivisionRepository,
            EmployeeRepository = EmployeeRepository,
            CalendarRepository = CalendarRepository,
            PayrollCalculatorProvider = PayrollCalculatorProvider,
            WebhookDispatchService = WebhookDispatchService,
            SourceCaseSet = sourceCaseSet,
            TargetCaseSet = targetCaseSet
        };

        // case relation build scripts
        foreach (var buildScripts in derivedCaseRelation.GetDerivedExpressionObjects(x => x.BuildScript))
        {
            var build = new CaseRelationScriptController().CaseRelationBuild(buildScripts, settings);
            if (build.HasValue)
            {
                return build.Value;
            }
        }
        return true;
    }
}