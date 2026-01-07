using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the collector apply script
/// </summary>
public class CollectorApplyRuntime : CollectorRuntimeBase, ICollectorApplyRuntime
{
    /// <summary>
    /// The wage type result
    /// </summary>
    private Model.WageTypeResult WageTypeResult { get; }

    internal CollectorApplyRuntime(Model.WageTypeResult wageTypeResult, CollectorRuntimeSettings settings) :
        base(settings)
    {
        WageTypeResult = wageTypeResult ?? throw new ArgumentNullException(nameof(wageTypeResult));
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CollectorApplyFunction);

    /// <summary>The wage type number</summary>
    public decimal WageTypeNumber => WageTypeResult.WageTypeNumber;

    /// <summary>The wage type name</summary>
    public string WageTypeName => WageTypeResult.WageTypeName;

    /// <summary>The wage type result value</summary>
    public decimal WageTypeValue => WageTypeResult.Value;

    /// <inheritdoc />
    public string[] GetApplyActions() =>
        Collector.ApplyActions == null ? [] :
            Collector.ApplyActions.ToArray();

    /// <summary>
    /// Execute the collector apply script
    /// </summary>
    /// <param name="collector">The collector</param>
    /// <returns>The collector value</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal decimal? ExecuteApplyScript(Collector collector)
    {
        try
        {
            var task = Task.Factory.StartNew<decimal?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(CollectorApplyFunction), collector);
                var value = ScriptValueConvert.ToDecimalValue(script.GetValue());
                return value;
            });
            return task.WaitScriptResult(typeof(CollectorApplyFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Apply script error in collector {collector.Name}: {exception.GetBaseMessage()}.", exception);
        }
    }
}