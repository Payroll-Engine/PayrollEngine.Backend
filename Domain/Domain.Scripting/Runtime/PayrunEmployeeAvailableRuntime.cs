using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the payrun employee available function
/// </summary>
public class PayrunEmployeeAvailableRuntime : PayrunRuntime, IPayrunEmployeeAvailableRuntime
{
    internal PayrunEmployeeAvailableRuntime(PayrunRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(PayrunEmployeeAvailableFunction);

    /// <summary>
    /// Execute the payrun employee available value script
    /// </summary>
    /// <param name="payrun">The payrun</param>
    /// <returns>True if the employee is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteIsEmployeeAvailableScript(Payrun payrun)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(PayrunEmployeeAvailableFunction), payrun);
                // call the script function
                return script.IsAvailable();
            });
            return task.WaitScriptResult(typeof(PayrunEmployeeAvailableFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Employee available script error in payrun {payrun.Name}: {exception.GetBaseMessage()}", exception);
        }
    }
}