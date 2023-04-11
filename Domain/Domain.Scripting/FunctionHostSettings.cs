using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Setting for the <see cref="FunctionHost"/>
/// </summary>
public class FunctionHostSettings
{
    public ITaskRepository TaskRepository { get; set; }
    public ILogRepository LogRepository { get; set; }
    /// <summary>Function log timeout</summary>
    public TimeSpan AssemblyCacheTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public IScriptProvider ScriptProvider { get; set; }
}