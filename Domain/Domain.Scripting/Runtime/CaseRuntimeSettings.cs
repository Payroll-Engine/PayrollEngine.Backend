using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class CaseRuntimeSettings : PayrollRuntimeSettings
{
    /// <summary>The case</summary>
    public Case Case { get; init; }
}