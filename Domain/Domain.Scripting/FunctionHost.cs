using System;
using System.Reflection;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <inheritdoc/>
public class FunctionHost : IFunctionHost
{
    public FunctionHostSettings Settings { get; }
    public ITaskRepository TaskRepository => Settings.TaskRepository;
    public ILogRepository LogRepository => Settings.LogRepository;

    /// <summary>
    /// The function log level, default is information
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    private readonly AssemblyCache assemblyCache;
    private bool disposed;

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
    ~FunctionHost()
    {
        // Finalizer calls Dispose(false)
        Dispose(false);
    }

    /// <inheritdoc/>
    public Assembly GetObjectAssembly(Type type, IScriptObject scriptObject) =>
        assemblyCache.GetObjectAssembly(type, scriptObject);

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

    /// <inheritdoc />
    /// <summary>
    /// Public implementation of Dispose pattern
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            disposed = true;
        }
    }
}