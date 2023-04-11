using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the case relation validate function
/// </summary>
public class CaseRelationValidateRuntime : CaseRelationRuntime, ICaseRelationValidateRuntime
{
    /// <summary>
    /// The validation issues
    /// </summary>
    private ICollection<CaseValidationIssue> Issues { get; }

    /// <inheritdoc />
    internal CaseRelationValidateRuntime(CaseRelation caseRelation, CaseRelationRuntimeSettings settings,
        ICollection<CaseValidationIssue> issues) :
        base(caseRelation, settings)
    {
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CaseRelationValidateFunction);

    /// <inheritdoc />
    public string[] GetValidateActions() =>
        CaseRelation.ValidateActions == null ? Array.Empty<string>() :
            CaseRelation.ValidateActions.ToArray();

    /// <inheritdoc />
    public bool HasIssues() => Issues.Any();

    /// <inheritdoc />
    public void AddIssue(string message, int number)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(nameof(message));
        }

        Issues.Add(new()
        {
            IssueType = CaseIssueType.CaseRelationInvalid,
            Number = number,
            SourceCaseName = SourceCaseSet.Name,
            SourceCaseNameLocalizations = SourceCaseSet.NameLocalizations,
            SourceCaseSlot = SourceCaseSet.CaseSlot,
            SourceCaseSlotLocalizations = SourceCaseSet.CaseSlotLocalizations,
            TargetCaseName = TargetCaseSet.Name,
            TargetCaseNameLocalizations = TargetCaseSet.NameLocalizations,
            TargetCaseSlot = TargetCaseSet.CaseSlot,
            TargetCaseSlotLocalizations = TargetCaseSet.CaseSlotLocalizations,
            Message = message
        });
    }

    /// <inheritdoc />
    public void AddIssue(string message, string caseFieldName, int number)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(nameof(message));
        }
        var caseField = CaseValueProvider.CaseFieldProvider.GetCaseFieldAsync(caseFieldName).Result;
        if (caseField == null)
        {
            throw new ScriptException($"Unknown case field {caseFieldName}");
        }

        Issues.Add(new()
        {
            IssueType = CaseIssueType.CaseRelationInvalid,
            Number = number,
            CaseFieldName = caseField.Name,
            CaseFieldNameLocalizations = caseField.NameLocalizations,
            SourceCaseName = SourceCaseSet.Name,
            SourceCaseNameLocalizations = SourceCaseSet.NameLocalizations,
            SourceCaseSlot = SourceCaseSet.CaseSlot,
            SourceCaseSlotLocalizations = SourceCaseSet.CaseSlotLocalizations,
            TargetCaseName = TargetCaseSet.Name,
            TargetCaseNameLocalizations = TargetCaseSet.NameLocalizations,
            TargetCaseSlot = TargetCaseSet.CaseSlot,
            TargetCaseSlotLocalizations = TargetCaseSet.CaseSlotLocalizations,
            Message = message
        });
    }

    /// <summary>
    /// Execute the case relation validate script
    /// </summary>
    /// <param name="caseRelation">The case relation</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteCaseRelationValidateScript(CaseRelation caseRelation)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(CaseRelationValidateFunction), caseRelation);
                // call the script function
                return script.Validate();
            });
            return task.WaitScriptResult(typeof(CaseRelationValidateFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Validate script error in case relation {caseRelation}: {exception.GetBaseMessage()}", exception);
        }
    }
}