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
/// Runtime for the wage type value function
/// </summary>
public class WageTypeValueRuntime : WageTypeRuntimeBase, IWageTypeValueRuntime
{
    /// <summary>
    /// Provider for case fields
    /// </summary>
    protected ICaseFieldProvider CaseFieldProvider { get; }

    /// <inheritdoc />
    internal WageTypeValueRuntime(ICaseFieldProvider caseFieldProvider, WageTypeRuntimeSettings settings) :
        base(settings)
    {
        CaseFieldProvider = caseFieldProvider ?? throw new ArgumentNullException(nameof(caseFieldProvider));
    }

    /// <inheritdoc />
    public int ExecutionCount => Settings.ExecutionCount;

    // execution restart
    internal bool ExecutionRestartRequest { get; private set; }

    /// <inheritdoc />
    public void RestartExecution() => ExecutionRestartRequest = true;

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(WageTypeValueFunction);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal decimal? EvaluateValue(WageType wageType)
    {
        try
        {
            var task = Task.Factory.StartNew<decimal?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(WageTypeValueFunction), wageType);
                /* *** test ***
                if ("WageTypeName".Equals(wageType.Name))
                {
                    // test ops
                }
                */
                return ScriptValueConvert.ToDecimalValue(script.GetValue());
            });
            return task.WaitScriptResult(typeof(WageTypeValueFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Value script error in wage type {wageType.WageTypeNumber.ToString("0.####", CultureInfo.InvariantCulture)}: {exception.GetBaseMessage()}", exception);
        }
    }
}