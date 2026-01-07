// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>Derived case relation</summary>
public sealed class DerivedCaseRelation : CaseRelation
{
    /// <summary>The regulation id</summary>
    public int RegulationId { get; set; }

    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}