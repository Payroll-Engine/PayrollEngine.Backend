using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the payrun employee end function
/// </summary>
public class PayrunEmployeeEndRuntime : PayrunRuntimeBase, IPayrunEmployeeEndRuntime
{
    internal PayrunEmployeeEndRuntime(PayrunRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(PayrunEmployeeEndFunction);

    /// <summary>
    /// Execute the payrun employee end value script
    /// </summary>
    /// <param name="payrun">The payrun</param>
    /// <returns>True if the employee is end</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void ExecuteEmployeeEndScript(Payrun payrun)
    {
        try
        {
            var task = Task.Factory.StartNew(() =>
            {
                // create script
                using var script = CreateScript(typeof(PayrunEmployeeEndFunction), payrun);
                // call the script function
                script.End();
            });
            task.Wait(Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Employee end script error in payrun {payrun.Name}: {exception.GetBaseMessage()}.", exception);
        }
    }
}