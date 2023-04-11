using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A case value change
/// </summary>
public class CaseValueChange : DomainObjectBase, IEquatable<CaseValueChange>
{
    /// <summary>
    /// The case change id
    /// </summary>
    public int CaseChangeId { get; set; }

    /// <summary>
    /// The case value id
    /// </summary>
    public int CaseValueId { get; set; }

    /// <inheritdoc/>
    public CaseValueChange()
    {
    }

    /// <inheritdoc/>
    public CaseValueChange(CaseValueChange copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseValueChange compare) =>
        CompareTool.EqualProperties(this, compare);
}