using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the report start function
/// </summary>
public class ReportStartRuntime : ReportRuntime, IReportStartRuntime
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportStartRuntime"/> class.
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    public ReportStartRuntime(ReportRuntimeSettings settings) :
        base(settings)
    {
        // ensure queries
        Report.Queries ??= new();
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(ReportStartFunction);

    /// <inheritdoc />
    public void SetParameter(string parameterName, string value) =>
        SetParameterInternal(parameterName, value);

    /// <inheritdoc />
    public bool HasQuery(string queryName) =>
        Report.Queries.ContainsKey(queryName);

    /// <inheritdoc />
    public string GetQuery(string queryName) =>
        Report.Queries[queryName];

    /// <inheritdoc />
    public void SetQuery(string queryName, string value) =>
        Report.Queries[queryName] = value;

    /// <summary>
    /// Execute the report start script
    /// </summary>
    /// <param name="report">The report</param>
    /// <returns>True if the employee is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void ExecuteStartScript(ReportSet report)
    {
        try
        {
            var task = Task.Factory.StartNew(() =>
            {
                // create script
                using var script = CreateScript(typeof(ReportStartFunction), report);
                // call the script function
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
            throw new ScriptException($"Start script error in report {report.Name}: {exception.GetBaseMessage()}", exception);
        }
    }
}