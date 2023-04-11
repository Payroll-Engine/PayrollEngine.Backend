using System;
using System.Reflection;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provide an assembly
/// </summary>
public interface IFunctionHost : IDisposable
{
    /// <summary>
    /// The function log level
    /// </summary>
    LogLevel LogLevel { get; set; }

    /// <summary>
    /// Get object script assembly
    /// </summary>
    /// <param name="type">The object type</param>
    /// <param name="scriptObject">The scripting object</param>
    /// <returns>The assembly</returns>
    Assembly GetObjectAssembly(Type type, IScriptObject scriptObject);

    /// <summary>Add task</summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="task">The task to add</param>
    void AddTask(int tenantId, Task task);

    /// <summary>Add a log</summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="log">The log to add</param>
    void AddLog(int tenantId, Model.Log log);
}