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
/// Runtime for the collector start script
/// </summary>
public class CollectorStartRuntime : CollectorRuntimeBase, ICollectorStartRuntime
{

    internal CollectorStartRuntime(CollectorRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CollectorStartFunction);

    /// <inheritdoc />
    public decimal[] GetValues() =>
        Collector.GetValues().ToArray();

    /// <inheritdoc />
    public void SetValues(decimal[] values) =>
        Collector.SetValues(values);

    /// <inheritdoc />
    public string[] GetStartActions() =>
        Collector.StartActions == null ? [] :
            Collector.StartActions.ToArray();

    /// <summary>
    /// Execute the collector start script
    /// </summary>
    /// <param name="collector">The collector</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void ExecuteStartScript(Collector collector)
    {
        try
        {
            var task = Task.Factory.StartNew(() =>
            {
                // create script
                using var script = CreateScript(typeof(CollectorStartFunction), collector);
                // ignore dummy return value
                script.Start();
            });
            task.Wait(Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Start script error in collector {collector.Name}: {exception.GetBaseMessage()}.", exception);
        }
    }
}