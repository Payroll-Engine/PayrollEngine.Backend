﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public class DerivedCaseValidator : DerivedCaseTool
{
    /// <summary>
    /// Constructor for global cases
    /// </summary>
    public DerivedCaseValidator(
        IGlobalCaseValueRepository globalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, settings)
    {
    }

    /// <summary>
    /// Constructor for national cases
    /// </summary>
    public DerivedCaseValidator(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, nationalCaseValueRepository, settings)
    {
    }

    /// <summary>
    /// Constructor for company cases
    /// </summary>
    public DerivedCaseValidator(
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(globalCaseValueRepository, nationalCaseValueRepository, companyCaseValueRepository,
            settings)
    {
    }

    /// <summary>
    /// Constructor for employee cases
    /// </summary>
    public DerivedCaseValidator(Employee employee,
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        IEmployeeCaseValueRepository employeeCaseValueRepository,
        DerivedCaseToolSettings settings) :
        base(employee, globalCaseValueRepository, nationalCaseValueRepository, companyCaseValueRepository,
            employeeCaseValueRepository, settings)
    {
    }

    public async Task<List<CaseValidationIssue>> ValidateGlobalCaseAsync(string caseName, string caseSlot,
        CaseChangeSetup caseChangeSetup, DateTime? cancellationDate = null) =>
        await ValidateCaseAsync(CaseType.Global, caseName, caseSlot, caseChangeSetup, cancellationDate);

    public async Task<List<CaseValidationIssue>> ValidateNationalCaseAsync(string caseName, string caseSlot,
        CaseChangeSetup caseChangeSetup, DateTime? cancellationDate = null) =>
        await ValidateCaseAsync(CaseType.National, caseName, caseSlot, caseChangeSetup, cancellationDate);

    public async Task<List<CaseValidationIssue>> ValidateCompanyCaseAsync(string caseName, string caseSlot,
        CaseChangeSetup caseChangeSetup, DateTime? cancellationDate = null) =>
        await ValidateCaseAsync(CaseType.Company, caseName, caseSlot, caseChangeSetup, cancellationDate);

    public async Task<List<CaseValidationIssue>> ValidateEmployeeCaseAsync(string caseName, string caseSlot,
        CaseChangeSetup caseChangeSetup, DateTime? cancellationDate = null) =>
        await ValidateCaseAsync(CaseType.Employee, caseName, caseSlot, caseChangeSetup, cancellationDate);

    private async Task<List<CaseValidationIssue>> ValidateCaseAsync(CaseType caseType, string caseName,
        string caseSlot, CaseChangeSetup caseChangeSetup, DateTime? cancellationDate = null)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }
        if (caseChangeSetup == null)
        {
            throw new ArgumentNullException(nameof(caseChangeSetup));
        }
        if (caseChangeSetup.CancellationId.HasValue && !cancellationDate.HasValue)
        {
            throw new ArgumentException("Cancellation case without cancellation date", nameof(caseChangeSetup));
        }

        var issues = new List<CaseValidationIssue>();

        // case value dictionary lookup
        var duplicatedCaseValue = caseChangeSetup.FindDuplicatedCaseValue();
        if (duplicatedCaseValue != null)
        {
            var message = string.IsNullOrWhiteSpace(duplicatedCaseValue.CaseSlot) ?
                $"Case field {duplicatedCaseValue.CaseFieldName} with multiple values" :
                $"Case field {duplicatedCaseValue.CaseFieldName} slot {duplicatedCaseValue.CaseSlot} with multiple values";
            issues.Add(new()
            {
                IssueType = CaseIssueType.CaseFieldDuplicated,
                Number = (int)CaseIssueType.CaseFieldDuplicated * -1,
                CaseName = duplicatedCaseValue.CaseName,
                CaseNameLocalizations = duplicatedCaseValue.CaseNameLocalizations,
                CaseSlot = duplicatedCaseValue.CaseSlot,
                CaseSlotLocalizations = duplicatedCaseValue.CaseSlotLocalizations,
                CaseFieldName = duplicatedCaseValue.CaseFieldName,
                CaseFieldNameLocalizations = duplicatedCaseValue.CaseFieldNameLocalizations,
                Message = message
            });
            Log.Trace(message);
            return issues;
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
            caseNames: new[] { caseName },
            clusterSet: ClusterSet,
            overrideType: OverrideType.Active)).ToList();
        if (!cases.Any())
        {
            var message = $"Missing case {caseName} in payroll with id {Payroll.Id}";
            issues.Add(new()
            {
                IssueType = CaseIssueType.CaseUnknown,
                Number = (int)CaseIssueType.CaseUnknown * -1,
                CaseName = caseName,
                // no localizations available
                Message = message
            });
            Log.Trace(message);
            return issues;
        }

        // derived case set from the most derived one
        var caseSet = await GetDerivedCaseSetAsync(cases, caseSlot, caseChangeSetup, default, false);

        // case cancellation
        if (caseChangeSetup.CancellationId.HasValue)
        {
            caseSet.SetCancellationDate(cancellationDate);
        }

        // support unknown related cases only in cancellation mode
        var ignoreUnknownRelations = cancellationDate == null;
        // resolve case: start of recursion
        await ValidateCaseAsync(cases, caseSet, caseChangeSetup, issues, default, ignoreUnknownRelations);

        // apply runtime case values
        var fields = caseSet.CollectFields();
        foreach (var field in fields)
        {
            // validate case field
            var caseFieldIssues = ValidateCaseField(caseSet, field);
            if (caseFieldIssues != null && caseFieldIssues.Any())
            {
                issues.AddRange(caseFieldIssues);
                continue;
            }

            // case value
            var caseValue = caseChangeSetup.FindCaseValue(field.Name, field.CaseSlot);

            // dynamic created case value
            if (caseValue == null)
            {
                // ignore undefined values
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    caseValue = new()
                    {
                        DivisionId = Payroll.DivisionId,
                        CaseName = caseSet.Name,
                        CaseNameLocalizations = caseSet.NameLocalizations,
                        CaseFieldName = field.Name,
                        CaseFieldNameLocalizations = field.NameLocalizations,
                        CaseSlot = field.CaseSlot,
                        ValueType = field.ValueType,
                        Value = field.Value,
                        Start = field.Start,
                        End = field.End,
                        CancellationDate = field.CancellationDate,
                        Tags = field.Tags,
                        Attributes = field.ValueAttributes
                    };
                    if (caseChangeSetup.Created.HasValue)
                    {
                        caseValue.Created = caseChangeSetup.Created.Value;
                        caseValue.Updated = caseValue.Created;
                    }

                    var caseSetup = caseChangeSetup.FindCaseSetup(caseSet.Name, field.CaseSlot);
                    if (caseSetup != null)
                    {
                        // add to existing case
                        caseSetup.Values.Add(caseValue);
                    }
                    else
                    {
                        // added to root case
                        caseChangeSetup.Case.Values.Add(caseValue);
                    }
                }
            }
            else
            {
                // update case values, keep existing tags and attributes
                caseValue.Start = field.Start;
                caseValue.End = field.End;
                caseValue.Value = field.Value;
                caseValue.CancellationDate = field.CancellationDate;
                caseValue.Tags ??= field.Tags;
                caseValue.Attributes ??= field.ValueAttributes;
            }
        }

        return issues;
    }

    private List<CaseValidationIssue> ValidateCaseField(Case @case, CaseFieldSet caseFieldSet)
    {
        var issues = new List<CaseValidationIssue>();

        // start date type
        if (caseFieldSet.Start.HasValue)
        {
            if (!caseFieldSet.StartDateType.IsStartMatching(caseFieldSet.Start.Value))
            {
                issues.Add(new()
                {
                    IssueType = CaseIssueType.CaseValueStartInvalid,
                    Number = (int)CaseIssueType.CaseValueStartInvalid * -1,
                    CaseName = @case.Name,
                    CaseNameLocalizations = @case.NameLocalizations,
                    CaseSlot = caseFieldSet.CaseSlot,
                    CaseSlotLocalizations = caseFieldSet.CaseSlotLocalizations,
                    CaseFieldName = caseFieldSet.Name,
                    CaseFieldNameLocalizations = caseFieldSet.NameLocalizations,
                    Message = $"Case field {caseFieldSet.Name}: invalid start date {caseFieldSet.Start.Value.ToPeriodStartString()}, expected {caseFieldSet.StartDateType}"
                });
            }
        }

        // end date type
        if (caseFieldSet.End.HasValue)
        {
            if (!caseFieldSet.EndDateType.IsEndMatching(caseFieldSet.End.Value))
            {
                issues.Add(new()
                {
                    IssueType = CaseIssueType.CaseValueEndInvalid,
                    Number = (int)CaseIssueType.CaseValueEndInvalid * -1,
                    CaseName = @case.Name,
                    CaseNameLocalizations = @case.NameLocalizations,
                    CaseSlot = caseFieldSet.CaseSlot,
                    CaseSlotLocalizations = caseFieldSet.CaseSlotLocalizations,
                    CaseFieldName = caseFieldSet.Name,
                    CaseFieldNameLocalizations = caseFieldSet.NameLocalizations,
                    Message = $"Case field {caseFieldSet.Name}: invalid end date {caseFieldSet.End.Value.ToPeriodStartString()}, expected {caseFieldSet.EndDateType}"
                });
            }
        }

        // mandatory end date
        if (caseFieldSet.EndMandatory && !caseFieldSet.End.HasValue)
        {
            issues.Add(new()
            {
                IssueType = CaseIssueType.CaseValueEndMissing,
                Number = (int)CaseIssueType.CaseValueEndMissing * -1,
                CaseName = @case.Name,
                CaseNameLocalizations = @case.NameLocalizations,
                CaseSlot = caseFieldSet.CaseSlot,
                CaseSlotLocalizations = caseFieldSet.CaseSlotLocalizations,
                CaseFieldName = caseFieldSet.Name,
                CaseFieldNameLocalizations = caseFieldSet.NameLocalizations,
                Message = $"Case field {caseFieldSet.Name}: missing mandatory end date"
            });
        }

        // mandatory value
        if (caseFieldSet.ValueType != ValueType.None && caseFieldSet.ValueMandatory && !caseFieldSet.HasValue)
        {
            issues.Add(new()
            {
                IssueType = CaseIssueType.CaseValueMissing,
                Number = (int)CaseIssueType.CaseValueMissing * -1,
                CaseName = @case.Name,
                CaseNameLocalizations = @case.NameLocalizations,
                CaseSlot = caseFieldSet.CaseSlot,
                CaseSlotLocalizations = caseFieldSet.CaseSlotLocalizations,
                CaseFieldName = caseFieldSet.Name,
                CaseFieldNameLocalizations = caseFieldSet.NameLocalizations,
                Message = $"Case field {caseFieldSet.Name}: missing mandatory value"
            });
        }

        // weekday value
        if (caseFieldSet.ValueType == ValueType.Weekday && caseFieldSet.HasValue)
        {
            if (!(caseFieldSet.GetValue() is int value) || !Enum.IsDefined(typeof(DayOfWeek), value))
            {
                issues.Add(new()
                {
                    IssueType = CaseIssueType.CaseValueWeekdayInvalid,
                    Number = (int)CaseIssueType.CaseValueWeekdayInvalid * -1,
                    CaseName = @case.Name,
                    CaseNameLocalizations = @case.NameLocalizations,
                    CaseSlot = caseFieldSet.CaseSlot,
                    CaseSlotLocalizations = caseFieldSet.CaseSlotLocalizations,
                    CaseFieldName = caseFieldSet.Name,
                    CaseFieldNameLocalizations = caseFieldSet.NameLocalizations,
                    Message = $"Case field {caseFieldSet.Name} with invalid weekday: {caseFieldSet.GetValue()}"
                });
            }
        }

        // month value
        if (caseFieldSet.ValueType == ValueType.Month && caseFieldSet.HasValue)
        {
            if (!(caseFieldSet.GetValue() is int value) || !Enum.IsDefined(typeof(Month), value))
            {
                issues.Add(new()
                {
                    IssueType = CaseIssueType.CaseValueMonthInvalid,
                    Number = (int)CaseIssueType.CaseValueMonthInvalid * -1,
                    CaseName = @case.Name,
                    CaseNameLocalizations = @case.NameLocalizations,
                    CaseSlot = caseFieldSet.CaseSlot,
                    CaseSlotLocalizations = caseFieldSet.CaseSlotLocalizations,
                    CaseFieldName = caseFieldSet.Name,
                    CaseFieldNameLocalizations = caseFieldSet.NameLocalizations,
                    Message = $"Case field {caseFieldSet.Name} with invalid month: {caseFieldSet.GetValue()}"
                });
            }
        }

        return issues;
    }

    // entry point to resolve recursive 
    private async Task ValidateCaseAsync(IList<Case> cases, CaseSet caseSet,
        CaseChangeSetup caseChangeSetup, List<CaseValidationIssue> issues, string culture, bool ignoreUnknownRelations)
    {
        // case validation
        await CaseValidateAsync(cases, caseSet, issues);

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
            return;
        }

        // setup related cases
        caseSet.RelatedCases = new();

        // group by relation
        var targetRelations = relations.GroupBy(x => new { x.SourceCaseName, x.SourceCaseSlot, x.TargetCaseName, x.TargetCaseSlot });
        foreach (var targetRelation in targetRelations)
        {
            // ignore unknown related cases (maybe not available/mapped)
            if (ignoreUnknownRelations && caseChangeSetup.FindCaseSetup(targetRelation.Key.TargetCaseName, targetRelation.Key.TargetCaseSlot) == null)
            {
                continue;
            }

            // target case (derived)
            var targetCase = (await PayrollRepository.GetDerivedCasesAsync(Settings.DbContext,
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
            if (!targetCase.Any())
            {
                throw new PayrollException($"Unknown related case with name {targetRelation.Key} in derived case {caseSet.Name}");
            }
            // target derived case set, the most derived one
            var targetCaseSet = await GetDerivedCaseSetAsync(targetCase, targetRelation.Key.TargetCaseSlot, caseChangeSetup, culture, false);
            caseSet.RelatedCases.Add(targetCaseSet);

            // ensure recursive cancellation date
            targetCaseSet.CancellationDate = caseSet.CancellationDate;

            // validate case relation
            if (await CaseRelationValidateAsync(targetRelation.ToList(), caseSet, targetCaseSet, issues))
            {
                // process related case (recursive)
                await ValidateCaseAsync(targetCase, targetCaseSet, caseChangeSetup, issues, culture, ignoreUnknownRelations);
            }
        }

        // case complete, test this at the end
        ValidateCompleteCase(caseSet, issues);
    }

    private static void ValidateCompleteCase(CaseSet caseSet, ICollection<CaseValidationIssue> caseIssues)
    {
        // test for complete fields
        if (caseSet.Fields != null)
        {
            foreach (var field in caseSet.Fields)
            {
                if (!field.IsComplete())
                {
                    caseIssues.Add(new()
                    {
                        IssueType = CaseIssueType.CaseValueIncomplete,
                        Number = (int)CaseIssueType.CaseValueIncomplete * -1,
                        CaseName = caseSet.Name,
                        CaseNameLocalizations = caseSet.NameLocalizations,
                        CaseSlot = field.CaseSlot,
                        CaseSlotLocalizations = field.CaseSlotLocalizations,
                        CaseFieldName = field.Name,
                        CaseFieldNameLocalizations = field.NameLocalizations,
                        Message = $"Incomplete case field {field.Name} in case {caseSet.Name}"
                    });
                }
            }
        }
    }

    private async Task CaseValidateAsync(IEnumerable<Case> cases, CaseSet caseSet, List<CaseValidationIssue> caseIssues)
    {
        var lookupProvider = await NewRegulationLookupProviderAsync();

        // case validate expression
        foreach (var validateScripts in cases.GetDerivedExpressionObjects(x => x.ValidateScript))
        {
            // issues may be added by the script
            var issues = new List<CaseValidationIssue>();
            var valid = new CaseScriptController().CaseValidate(validateScripts, new()
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
                WebhookDispatchService = WebhookDispatchService,
                Case = caseSet
            }, issues);

            // issues
            if (issues.Any())
            {
                caseIssues.AddRange(issues);
                return;
            }
            // failed function result
            if (valid.HasValue && !valid.Value)
            {
                caseIssues.Add(new()
                {
                    IssueType = CaseIssueType.CaseInvalid,
                    Number = (int)CaseIssueType.CaseInvalid * -1,
                    CaseName = caseSet.Name,
                    CaseNameLocalizations = caseSet.NameLocalizations,
                    CaseSlot = caseSet.CaseSlot,
                    CaseSlotLocalizations = caseSet.CaseSlotLocalizations,
                    Message = $"Case {caseSet.Name} validation failed"
                });
                return;
            }
        }
    }

    private async Task<bool> CaseRelationValidateAsync(IEnumerable<CaseRelation> derivedCaseRelation, CaseSet sourceCaseSet,
        CaseSet targetCaseSet, List<CaseValidationIssue> caseRelationIssues)
    {
        var lookupProvider = await NewRegulationLookupProviderAsync();

        // case relation validate scripts
        foreach (var validateScripts in derivedCaseRelation.GetDerivedExpressionObjects(x => x.ValidateScript))
        {
            // issues may be added by the script
            var issues = new List<CaseValidationIssue>();
            var valid = new CaseRelationScriptController().CaseRelationValidate(validateScripts, new()
            {
                DbContext = Settings.DbContext,
                UserCulture = UserCulture,
                FunctionHost = FunctionHost,
                Tenant = Tenant,
                User = User,
                Payroll = Payroll,
                CaseValueProvider = CaseValueProvider,
                RegulationLookupProvider = lookupProvider,
                WebhookDispatchService = WebhookDispatchService,
                SourceCaseSet = sourceCaseSet,
                TargetCaseSet = targetCaseSet
            }, issues);

            // issues
            if (issues.Any())
            {
                caseRelationIssues.AddRange(issues);
                return false;
            }
            // failed function result
            if (valid.HasValue && !valid.Value)
            {
                caseRelationIssues.Add(new()
                {
                    IssueType = CaseIssueType.CaseRelationInvalid,
                    SourceCaseName = validateScripts.SourceCaseName,
                    SourceCaseNameLocalizations = validateScripts.SourceCaseNameLocalizations,
                    SourceCaseSlot = validateScripts.SourceCaseSlot,
                    SourceCaseSlotLocalizations = validateScripts.SourceCaseSlotLocalizations,
                    TargetCaseName = validateScripts.TargetCaseName,
                    TargetCaseNameLocalizations = validateScripts.TargetCaseNameLocalizations,
                    TargetCaseSlot = validateScripts.TargetCaseSlot,
                    TargetCaseSlotLocalizations = validateScripts.TargetCaseSlotLocalizations,
                    Message = $"Case relation {validateScripts} validation failed"
                });
                return false;
            }
        }
        return true;
    }

}