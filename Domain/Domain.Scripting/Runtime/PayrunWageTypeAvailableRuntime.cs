using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the payrun wage type is available function
/// </summary>
public class PayrunWageTypeAvailableRuntime : PayrunRuntime, IPayrunWageTypeAvailableRuntime
{
    /// <summary>
    /// The wage type
    /// </summary>
    protected WageType WageType { get; }

    /// <summary>
    /// The derived wage type attributes
    /// </summary>
    protected Dictionary<string, object> WageTypeAttributes { get; }

    /// <inheritdoc />
    public decimal WageTypeNumber => WageType.WageTypeNumber;

    /// <inheritdoc />
    public object GetWageTypeAttribute(string attributeName) =>
        WageTypeAttributes[attributeName];

    /// <inheritdoc />
    internal PayrunWageTypeAvailableRuntime(WageType wageType, Dictionary<string, object> wageTypeAttributes,
        PayrunRuntimeSettings settings) :
        base(settings)
    {
        WageType = wageType ?? throw new ArgumentNullException(nameof(wageType));
        WageTypeAttributes = wageTypeAttributes ?? throw new ArgumentNullException(nameof(wageTypeAttributes));
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(PayrunWageTypeAvailableFunction);

    /// <summary>
    /// Execute the payrun wage type available value script
    /// </summary>
    /// <param name="payrun">The payrun</param>
    /// <returns>True if the wage type is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteIsWageTypeAvailableScript(Payrun payrun)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(PayrunWageTypeAvailableFunction), payrun);
                // call the script function
                return script.IsAvailable();
            });
            return task.WaitScriptResult(typeof(PayrunWageTypeAvailableFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Wage type available script error in payrun {payrun.Name}: {exception.GetBaseMessage()}", exception);
        }
    }
}