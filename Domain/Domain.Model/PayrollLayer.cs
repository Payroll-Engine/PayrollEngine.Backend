using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll layer
/// </summary>
public class PayrollLayer : DomainObjectBase, IDomainAttributeObject, IEquatable<PayrollLayer>
{
    /// <summary>
    /// The layer level
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// The layer priority (default: 1)
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// The regulation name
    /// </summary>
    public string RegulationName { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public PayrollLayer()
    {
    }

    /// <inheritdoc/>
    public PayrollLayer(PayrollLayer copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrollLayer compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Level}.{Priority} -> {RegulationName} {base.ToString()}";
}