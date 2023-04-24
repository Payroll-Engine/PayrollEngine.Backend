using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived case relation</summary>
public sealed class DerivedCaseRelation : CaseRelation
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}