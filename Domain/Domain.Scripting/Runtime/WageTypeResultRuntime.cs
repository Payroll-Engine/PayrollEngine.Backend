using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the wage type result function
/// </summary>
public class WageTypeResultRuntime : WageTypeRuntimeBase, IWageTypeResultRuntime
{
    /// <inheritdoc />
    public decimal WageTypeValue { get; }

    internal WageTypeResultRuntime(decimal wageTypeValue, WageTypeRuntimeSettings settings) :
        base(settings)
    {
        WageTypeValue = wageTypeValue;
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(WageTypeResultFunction);

    /// <inheritdoc />
    public string[] GetResultActions() =>
        WageType.ResultActions == null ? [] :
            WageType.ResultActions.ToArray();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void EvaluateResult(WageType wageType)
    {
        try
        {
            var task = Task.Factory.StartNew(() =>
            {
                // create script
                using var script = CreateScript(typeof(WageTypeResultFunction), wageType);
                // ignore dummy return value
                script.Result();
            });
            task.Wait(Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Result script error in wage type {wageType.WageTypeNumber.ToString("0.####", CultureInfo.InvariantCulture)}: {exception.GetBaseMessage()}", exception);
        }
    }
}