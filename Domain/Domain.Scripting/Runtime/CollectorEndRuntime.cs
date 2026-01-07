using System;
using System.Linq;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the collector end script
/// </summary>
public class CollectorEndRuntime : CollectorRuntimeBase, ICollectorEndRuntime
{
    internal CollectorEndRuntime(CollectorRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CollectorEndFunction);

    /// <inheritdoc />
    public decimal[] GetValues() =>
        Collector.GetValues().ToArray();

    /// <inheritdoc />
    public void SetValues(decimal[] values) =>
        Collector.SetValues(values);
    
    /// <inheritdoc />
    public string[] GetEndActions() =>
        Collector.EndActions == null ? [] :
            Collector.EndActions.ToArray();

    /// <summary>
    /// Execute the collector end script
    /// </summary>
    /// <param name="collector">The collector</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void ExecuteEndScript(Collector collector)
    {
        try
        {
            var task = Task.Factory.StartNew(() =>
            {
                // create script
                using var script = CreateScript(typeof(CollectorEndFunction), collector);
                // ignore dummy return value
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
            throw new ScriptException($"End script error in collector {collector.Name}: {exception.GetBaseMessage()}.", exception);
        }
    }
}