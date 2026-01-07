using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the case field validation function
/// </summary>
public class CaseValidateRuntime : CaseChangeRuntimeBase, ICaseValidateRuntime
{
    /// <summary>
    /// The validation issues
    /// </summary>
    private ICollection<CaseValidationIssue> Issues { get; }

    /// <inheritdoc />
    internal CaseValidateRuntime(CaseChangeRuntimeSettings settings, ICollection<CaseValidationIssue> issues) :
        base(settings)
    {
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CaseValidateFunction);

    /// <inheritdoc />
    public bool HasIssues() => Issues.Any();

    /// <inheritdoc />
    public void AddCaseIssue(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(nameof(message));
        }

        Issues.Add(new()
        {
            IssueType = CaseIssueType.CaseInvalid,
            Number = (int)CaseIssueType.CaseInvalid * -1,
            CaseName = Case.Name,
            CaseNameLocalizations = Case.NameLocalizations,
            CaseSlot = Case.CaseSlot,
            CaseSlotLocalizations = Case.CaseSlotLocalizations,
            Message = message
        });
    }

    /// <inheritdoc />
    public void AddCaseFieldIssue(string caseFieldName, string message)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(nameof(message));
        }

        // namespace
        caseFieldName = caseFieldName.EnsureNamespace(Namespace);

        // case field
        var caseField = GetCaseFieldSet(caseFieldName);
        if (caseField == null)
        {
            throw new ScriptException($"Unknown case field {caseFieldName}.");
        }

        Issues.Add(new()
        {
            IssueType = CaseIssueType.CaseInvalid,
            Number = (int)CaseIssueType.CaseInvalid * -1,
            CaseName = Case.Name,
            CaseNameLocalizations = Case.NameLocalizations,
            CaseSlot = Case.CaseSlot,
            CaseSlotLocalizations = Case.CaseSlotLocalizations,
            CaseFieldName = caseField.Name,
            CaseFieldNameLocalizations = caseField.NameLocalizations,
            Message = message
        });
    }

    /// <summary>
    /// Execute the case validate script
    /// </summary>
    /// <param name="case">The case</param>
    /// <returns>True if the case is valid</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteCaseValidateScript(Case @case)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(CaseValidateFunction), @case);
                // call the script function
                return script.Validate();
            });
            return task.WaitScriptResult(typeof(CaseValidateFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Validate script error in case {CaseName}: {exception.GetBaseMessage()}.", exception);
        }
    }
}