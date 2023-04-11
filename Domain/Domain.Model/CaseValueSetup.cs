using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A national, company and employee case value setup
/// </summary>
public class CaseValueSetup : CaseValue, IEquatable<CaseValueSetup>
{

    /// <summary>
    /// Case documents
    /// </summary>
    public List<CaseDocument> Documents { get; set; }

    /// <inheritdoc/>
    public CaseValueSetup()
    {
    }

    /// <inheritdoc/>
    public CaseValueSetup(CaseValue copySource) :
        base(copySource)
    {
    }

    /// <inheritdoc/>
    public CaseValueSetup(CaseValueSetup copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseValueSetup compare) =>
        CompareTool.EqualProperties(this, compare);
}