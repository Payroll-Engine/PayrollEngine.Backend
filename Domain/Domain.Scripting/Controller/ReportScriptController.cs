//#define SCRIPT_PERFORMANCE
#if SCRIPT_PERFORMANCE
#define LOG_STOPWATCH
#endif
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Report script controller
/// </summary>
public class ReportScriptController<T> : ScriptControllerBase<T>
    where T : Report
{
    public bool? Build(ReportRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(ReportBuildRuntime));

        // script runtime
        var runtime = new ReportBuildRuntime(settings);

        // script execution
        var build = runtime.ExecuteBuildScript(settings.Report);

        LogStopwatch.Stop(nameof(ReportBuildRuntime));

        return build;
    }

    public void Start(ReportRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(ReportStartRuntime));

        // script runtime
        var runtime = new ReportStartRuntime(settings);

        // script execution
        runtime.ExecuteStartScript(settings.Report);

        LogStopwatch.Stop(nameof(ReportStartRuntime));
    }

    public void End(ReportRuntimeSettings settings, System.Data.DataSet dataSet)
    {
        LogStopwatch.Start(nameof(ReportEndRuntime));

        // script runtime
        var runtime = new ReportEndRuntime(settings, dataSet);

        // script execution
        runtime.ExecuteEndScript(settings.Report);

        LogStopwatch.Stop(nameof(ReportEndRuntime));
    }
}