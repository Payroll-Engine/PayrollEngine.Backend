
namespace PayrollEngine.Api.Model;

/// <summary>
/// Case relation reference
/// </summary>
public class CaseRelationReference
{
    /// <summary>
    /// The relation source case name (immutable)
    /// </summary>
    public string SourceCaseName { get; set; }

    /// <summary>
    /// The relation source case slot
    /// </summary>
    public string SourceCaseSlot { get; set; }

    /// <summary>
    /// The relation target case name (immutable)
    /// </summary>
    public string TargetCaseName { get; set; }

    /// <summary>
    /// The relation target case slot
    /// </summary>
    public string TargetCaseSlot { get; set; }
}