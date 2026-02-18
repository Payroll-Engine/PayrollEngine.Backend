using System;
using System.Reflection;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <inheritdoc/>
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
        _ = TaskRepository.CreateAsync(Settings.DbContext, tenantId, task).Result;
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
        _ = LogRepository.CreateAsync(Settings.DbContext, tenantId, log).Result;
    }
}