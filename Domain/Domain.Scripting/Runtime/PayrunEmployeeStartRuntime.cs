using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the payrun employee start function
/// </summary>
public class PayrunEmployeeStartRuntime : PayrunRuntimeBase, IPayrunEmployeeStartRuntime
{
    internal PayrunEmployeeStartRuntime(PayrunRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(PayrunEmployeeStartFunction);

    /// <summary>
    /// Execute the payrun employee start value script
    /// </summary>
    /// <param name="payrun">The payrun</param>
    /// <returns>True if the employee is start</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteEmployeeStartScript(Payrun payrun)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(PayrunEmployeeStartFunction), payrun);
                // call the script function
                return script.Start();
            });
            return task.WaitScriptResult(typeof(PayrunEmployeeStartFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Employee start script error in payrun {payrun.Name}: {exception.GetBaseMessage()}", exception);
        }
    }
}