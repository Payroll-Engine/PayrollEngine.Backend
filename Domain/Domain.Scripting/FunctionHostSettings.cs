using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Setting for the <see cref="FunctionHost"/>
/// </summary>
public class FunctionHostSettings
{
    public IDbContext DbContext { get; init; }
    public ITaskRepository TaskRepository { get; init; }
    public ILogRepository LogRepository { get; init; }
    /// <summary>Function log timeout</summary>
    public TimeSpan AssemblyCacheTimeout { get; init; } = TimeSpan.FromMinutes(30);
    public IScriptProvider ScriptProvider { get; init; }
}