using System;
using System.Runtime.CompilerServices;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Client.Scripting.Function;

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
            throw new ScriptException($"Build script error in case {CaseName}: {exception.GetBaseMessage()}.", exception);
        }
    }
}