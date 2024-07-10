using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime to build a case
/// </summary>
public class CaseBuildRuntime : CaseChangeRuntimeBase, ICaseBuildRuntime
{
    internal CaseBuildRuntime(CaseChangeRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CaseBuildFunction);

    /// <inheritdoc />
    public string[] GetBuildActions() =>
        Case.BuildActions == null ? [] :
            Case.BuildActions.ToArray();

    /// <inheritdoc />
    public string[] GetFieldBuildActions(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // case field
        var caseField = GetCaseFieldSet(caseFieldName);
        if (caseField == null)
        {
            throw new ArgumentException($"unknown case field {caseFieldName}");
        }
        return caseField.BuildActions == null ? [] : caseField.BuildActions.ToArray();
    }

    /// <summary>
    /// Execute the case build script
    /// </summary>
    /// <param name="case">The case</param>
    /// <returns>True if the case is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteCaseBuildScript(Case @case)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(CaseBuildFunction), @case);

                // call the script function
                return script.Build();
            });
            return task.WaitScriptResult(typeof(CaseBuildFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Build script error in case {CaseName}: {exception.GetBaseMessage()}", exception);
        }
    }
}