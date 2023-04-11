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
/// Runtime for the report build function
/// </summary>
public class ReportBuildRuntime : ReportRuntime, IReportBuildRuntime
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportBuildRuntime"/> class
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    public ReportBuildRuntime(ReportRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(ReportBuildFunction);

    /// <inheritdoc />
    public void SetParameter(string parameterName, string value) =>
        SetParameterInternal(parameterName, value);

    /// <inheritdoc />
    public void SetParameterAttribute(string parameterName, string attributeName, object value)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException(nameof(parameterName));
        }
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }

        // report parameter
        if (Report.Parameters == null)
        {
            throw new ArgumentException($"Invalid report parameter {parameterName}");
        }
        var reportParameter = Report.Parameters.FirstOrDefault(x => string.Equals(x.Name, parameterName));
        if (reportParameter == null)
        {
            throw new ArgumentException($"Unknown report parameter {parameterName}");
        }

        // remove attribute
        if (value == null)
        {
            if (reportParameter.Attributes != null && reportParameter.Attributes.ContainsKey(attributeName))
            {
                reportParameter.Attributes.Remove(attributeName);
            }
        }
        else
        {
            // add/change attribute
            reportParameter.Attributes ??= new();
            reportParameter.Attributes[attributeName] = value;
        }
    }

    /// <summary>
    /// Execute the report build script
    /// </summary>
    /// <param name="report">The report</param>
    /// <returns>True if the employee is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteBuildScript(ReportSet report)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(ReportBuildFunction), report);
                // call the script function
                return script.Build();
            });
            return task.WaitScriptResult(typeof(ReportBuildFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Build script error in report {report.Name}: {exception.GetBaseMessage()}", exception);
        }
    }
}