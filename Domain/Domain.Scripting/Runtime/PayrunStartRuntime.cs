using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the payrun start function
/// </summary>
public class PayrunStartRuntime : PayrunRuntimeBase, IPayrunStartRuntime
{
    internal PayrunStartRuntime(PayrunRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(PayrunStartFunction);

    /// <summary>
    /// Execute the payrun start value script
    /// </summary>
    /// <param name="payrun">The payrun</param>
    /// <returns>True if the employee is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteStartScript(Payrun payrun)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(PayrunStartFunction), payrun);
                // call the script function
                return script.Start();
            });
            return task.WaitScriptResult(typeof(CaseAvailableFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Start script error in payrun {payrun.Name}: {exception.GetBaseMessage()}.", exception);
        }
    }
}