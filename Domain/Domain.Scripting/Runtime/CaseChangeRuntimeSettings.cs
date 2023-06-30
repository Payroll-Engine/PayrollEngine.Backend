using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class CaseChangeRuntimeSettings : CaseRuntimeSettings
{
    /// <summary>The case provider</summary>
    public ICaseProvider CaseProvider { get; init; }
}