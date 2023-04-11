//#define SCRIPT_PERFORMANCE
#if SCRIPT_PERFORMANCE
#define LOG_STOPWATCH
#endif
using System;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Collector script controller
/// </summary>
public class CollectorScriptController : ScriptControllerBase<Collector>, ICollectorScriptController
{
    /// <inheritdoc />
    public List<RetroPayrunJob> Start(CollectorRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(CollectorStartRuntime));

        // script runtime
        var runtime = new CollectorStartRuntime(settings);

        // script execution
        runtime.ExecuteStartScript(settings.Collector);

        LogStopwatch.Stop(nameof(CollectorStartRuntime));

        // payrun results
        settings.CurrentPayrollResult.PayrunResults.AddRange(runtime.PayrunResults);

        return runtime.RetroJobs;
    }

    /// <inheritdoc />
    public Tuple<decimal?, List<RetroPayrunJob>> ApplyValue(WageTypeResult wageTypeResult, CollectorRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(CollectorApplyRuntime));

        // script runtime
        var runtime = new CollectorApplyRuntime(wageTypeResult, settings);

        // script execution
        var value = runtime.ExecuteApplyScript(settings.Collector);

        LogStopwatch.Stop(nameof(CollectorApplyRuntime));

        // payrun results
        settings.CurrentPayrollResult.PayrunResults.AddRange(runtime.PayrunResults);

        // result
        return new(value, runtime.RetroJobs);
    }

    /// <inheritdoc />
    public List<RetroPayrunJob> End(CollectorRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(CollectorEndRuntime));

        // script runtime
        var runtime = new CollectorEndRuntime(settings);

        // script execution
        runtime.ExecuteEndScript(settings.Collector);

        LogStopwatch.Stop(nameof(CollectorEndRuntime));

        // payrun results
        settings.CurrentPayrollResult.PayrunResults.AddRange(runtime.PayrunResults);

        return runtime.RetroJobs;
    }
}