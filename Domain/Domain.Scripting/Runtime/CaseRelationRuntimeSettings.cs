using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class CaseRelationRuntimeSettings : PayrollRuntimeSettings
{
    /// <summary>
    /// The case values of the relation source
    /// </summary>
    public CaseSet SourceCaseSet { get; init; }

    /// <summary>
    /// The case values of the relation target
    /// </summary>
    public CaseSet TargetCaseSet { get; init; }
}