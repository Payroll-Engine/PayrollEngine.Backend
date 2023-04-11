using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Lookup including the lookup value
/// </summary>
public class LookupSet : Lookup, IEquatable<LookupSet>
{
    /// <summary>
    /// The lookup values
    /// </summary>
    public List<LookupValue> Values { get; set; }

    /// <inheritdoc/>
    public LookupSet()
    {
    }

    /// <inheritdoc/>
    public LookupSet(LookupSet copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupSet compare) =>
        CompareTool.EqualProperties(this, compare);
}