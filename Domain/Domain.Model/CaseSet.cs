using System;
using System.Collections.Generic;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case including case fields and related cases
/// </summary>
public class CaseSet : Case, IEquatable<CaseSet>
{
    /// <summary>
    /// The case display name
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slots
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// Cancellation date
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// Derived case fields
    /// </summary>
    public List<CaseFieldSet> Fields { get; set; }

    /// <summary>
    /// Related cases
    /// </summary>
    public List<CaseSet> RelatedCases { get; set; }

    /// <inheritdoc/>
    public CaseSet()
    {
    }

    /// <inheritdoc/>
    public CaseSet(CaseSet copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public CaseSet(Case copySource) :
        base(copySource)
    {
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseSet compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Fields?.Count} fields, {RelatedCases?.Count} related {base.ToString()}";
}