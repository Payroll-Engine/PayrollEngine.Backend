using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Report script controller
/// </summary>
public interface IReportScriptController : IReportScriptController<Report>
{
}

/// <summary>
/// Report script controller
/// </summary>
public interface IReportScriptController<out T> : IScriptController<T> where T : Report
{
    /// <summary>
    /// Execute report build script
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    bool? Build(ReportRuntimeSettings settings);

    /// <summary>
    /// Execute report start script
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    void Start(ReportRuntimeSettings settings);

    /// <summary>
    /// Execute report end script
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    /// <param name="dataSet">The report data set</param>
    void End(ReportRuntimeSettings settings, System.Data.DataSet dataSet);
}