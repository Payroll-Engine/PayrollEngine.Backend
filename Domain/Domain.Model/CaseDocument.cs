using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A case document
/// </summary>
public class CaseDocument : DomainObjectBase, IEquatable<CaseDocument>
{
    /// <summary>
    /// The document name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The document content
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The document content type
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseDocument"/> class
    /// </summary>
    public CaseDocument()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseDocument"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public CaseDocument(CaseDocument copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseDocument compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}