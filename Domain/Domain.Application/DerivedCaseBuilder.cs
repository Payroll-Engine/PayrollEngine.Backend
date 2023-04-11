using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;

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

    public virtual async Task<CaseSet> BuildGlobalCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, Language language) =>
        await BuildCaseAsync(CaseType.Global, caseName, caseChangeSetup, language);

    public virtual async Task<CaseSet> BuildNationalCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, Language language) =>
        await BuildCaseAsync(CaseType.National, caseName, caseChangeSetup, language);

    public virtual async Task<CaseSet> BuildCompanyCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, Language language) =>
        await BuildCaseAsync(CaseType.Company, caseName, caseChangeSetup, language);

    public virtual async Task<CaseSet> BuildEmployeeCaseAsync(string caseName,
        CaseChangeSetup caseChangeSetup, Language language) =>
        await BuildCaseAsync(CaseType.Employee, caseName, caseChangeSetup, language);

    protected virtual async Task<CaseSet> BuildCaseAsync(CaseType caseType, string caseName,
        CaseChangeSetup caseChangeSetup, Language language)
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
        var cases = (await PayrollRepository.GetDerivedCasesAsync(
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = Payroll.Id,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            caseType: caseType,
            caseNames: new[] { caseName },
            clusterSet: ClusterSet,
            overrideType: OverrideType.Active)).ToList();
        if (!cases.Any())
        {
            Log.Trace($"Missing case {caseName} in payroll with id {Payroll.Id}");
            return new();
        }

        // derived case from the most derived one
        var caseSlot = caseChangeSetup.Case?.CaseSlot;
        var caseSet = await GetDerivedCaseSetAsync(cases, caseSlot, caseChangeSetup, language, true);

        // resolve case: start of recursion
        await BuildCaseAsync(cases, caseSet, caseChangeSetup, language);

        return caseSet;
    }

    // entry point to resolve recursive
    private async Task<bool> BuildCaseAsync(IList<Case> cases, CaseSet caseSet,
        CaseChangeSetup caseChangeSetup, Language language)
    {
        var build = await CaseBuildAsync(cases, caseSet);
        if (!build)
        {
            Log.Trace($"Build failed for case {caseSet.Name}");
            return false;
        }

        // case relations (active only)
        var relations = (await PayrollRepository.GetDerivedCaseRelationsAsync(
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
        caseSet.RelatedCases = new();

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
            var targetCases = (await PayrollRepository.GetDerivedCasesAsync(
                new()
                {
                    TenantId = Tenant.Id,
                    PayrollId = Payroll.Id,
                    RegulationDate = RegulationDate,
                    EvaluationDate = EvaluationDate
                },
                caseNames: new[] { targetRelation.Key.TargetCaseName },
                clusterSet: ClusterSet,
                overrideType: OverrideType.Active)).ToList();
            if (!targetCases.Any())
            {
                throw new PayrollException($"Unknown related case with name {targetRelation.Key.TargetCaseName} in derived case {caseSet.Name}");
            }
            // target derived case on the most derived one
            var targetCaseSet = await GetDerivedCaseSetAsync(targetCases, targetRelation.Key.TargetCaseSlot, caseChangeSetup, language, true);

            // build case relation
            if (!await CaseRelationBuildAsync(targetRelation.ToList(), caseSet, targetCaseSet))
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
            if (await BuildCaseAsync(targetCases, targetCaseSet, caseChangeSetup, language))
            {
                // add related case (ignore invalid case)
                caseSet.RelatedCases.Add(targetCaseSet);
                Log.Trace($"Added related case {targetCaseSet.Name} to case {caseSet.Name}");
            }
        }

        return true;
    }

    private async Task<bool> CaseBuildAsync(IEnumerable<Case> cases, CaseSet caseSet)
    {
        var lookupProvider = await NewRegulationLookupProviderAsync();

        // case build expression
        var build = true;
        foreach (var buildScripts in cases.GetDerivedExpressionObjects(x => x.BuildScript))
        {
            var caseBuild = new CaseScriptController().CaseBuild(buildScripts, new()
            {
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = User,
                Payroll = Payroll,
                CaseProvider = CaseProvider,
                CaseValueProvider = CaseValueProvider,
                RegulationLookupProvider = lookupProvider,
                WebhookDispatchService = WebhookDispatchService,
                Case = caseSet
            });
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

    private async Task<bool> CaseRelationBuildAsync(IEnumerable<CaseRelation> derivedCaseRelation, CaseSet sourceCaseSet, CaseSet targetCaseSet)
    {
        var lookupProvider = await NewRegulationLookupProviderAsync();

        // case relation build scripts
        foreach (var buildScripts in derivedCaseRelation.GetDerivedExpressionObjects(x => x.BuildScript))
        {
            var build = new CaseRelationScriptController().CaseRelationBuild(buildScripts, new()
            {
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = User,
                Payroll = Payroll,
                CaseValueProvider = CaseValueProvider,
                RegulationLookupProvider = lookupProvider,
                WebhookDispatchService = WebhookDispatchService,
                SourceCaseSet = sourceCaseSet,
                TargetCaseSet = targetCaseSet
            });
            if (build.HasValue)
            {
                return build.Value;
            }
        }
        return true;
    }
}