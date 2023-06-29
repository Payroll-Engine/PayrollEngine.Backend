using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the case field available function
/// </summary>
public class CaseAvailableRuntime : CaseRuntimeBase, ICaseAvailableRuntime
{
    internal CaseAvailableRuntime(CaseRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CaseAvailableFunction);
    
    /// <inheritdoc />
    public string[] GetAvailableActions() =>
        Case.AvailableActions == null ? Array.Empty<string>() :
            Case.AvailableActions.ToArray();

    /// <summary>
    /// Execute the case available script
    /// </summary>
    /// <returns>True if the case is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteCaseAvailableScript()
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(CaseAvailableFunction), Case);
                // call the script function
                return script.IsAvailable();
            });
            return task.WaitScriptResult(typeof(CaseAvailableFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Available script error in case {CaseName}: {exception.GetBaseMessage()}", exception);
        }
    }
}