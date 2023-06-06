using System;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Extension methods for <see cref="TaskExtensions"/>
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Wait for the script result
    /// </summary>
    /// <param name="scriptTask">The script task</param>
    /// <param name="scriptType">The script type</param>
    /// <param name="timeout">The script execution timeout (TimeSpan.Zero=off)</param>
    /// <returns>The script result</returns>
    public static T WaitScriptResult<T>(this Task<T> scriptTask, Type scriptType, TimeSpan timeout)
    {
        if (timeout == TimeSpan.Zero)
        {
            scriptTask.Wait();
            return scriptTask.Result;
        }

        if (scriptTask.Wait(timeout))
        {
            return scriptTask.Result;
        }
        throw new TimeoutException($"The script function {scriptType.Name} has taken longer than the maximum time allowed ({(int)BackendScriptingSpecification.ScriptFunctionTimeout} ms)");
    }
}