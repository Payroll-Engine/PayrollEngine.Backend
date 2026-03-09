using System;
using System.Reflection;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <inheritdoc/>
/// <remarks>
/// This class bridges the synchronous <see cref="IFunctionHost"/> scripting API
/// with the async persistence layer. Sync-over-async via
/// <c>ConfigureAwait(false).GetAwaiter().GetResult()</c> is intentional here:
/// user scripts execute synchronously and cannot propagate async through the
/// compilation boundary. <c>ConfigureAwait(false)</c> avoids capturing any
/// ambient context, reducing the risk of deadlocks in hosted environments.
/// </remarks>
public class FunctionHost : IFunctionHost
{
    private FunctionHostSettings Settings { get; }
    private ITaskRepository TaskRepository => Settings.TaskRepository;
    private ILogRepository LogRepository => Settings.LogRepository;

    /// <summary>
    /// The function log level, default is information
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    private readonly AssemblyCache assemblyCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionHost"/> class
    /// </summary>
    /// <param name="settings">The host settings</param>
    public FunctionHost(FunctionHostSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        assemblyCache = new(settings.DbContext, settings.AssemblyCacheTimeout, settings.ScriptProvider);
    }

    /// <inheritdoc/>
    public Assembly GetObjectAssembly(int tenantId, Type type, IScriptObject scriptObject) =>
        assemblyCache.GetObjectAssembly(tenantId, type, scriptObject);

    /// <inheritdoc/>
    public void AddTask(int tenantId, Task task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }
        _ = TaskRepository.CreateAsync(Settings.DbContext, tenantId, task)
            .ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public void AddLog(int tenantId, Model.Log log)
    {
        if (log == null)
        {
            throw new ArgumentNullException(nameof(log));
        }

        // test if level is enabled
        if ((int)log.Level < (int)LogLevel)
        {
            return;
        }
        _ = LogRepository.CreateAsync(Settings.DbContext, tenantId, log)
            .ConfigureAwait(false).GetAwaiter().GetResult();
    }
}