using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Action parameter info
/// </summary>
public class ActionParameterInfo : IEquatable<ActionParameterInfo>
{
    /// <summary>The action parameter name</summary>
    public string Name { get; set; }

    /// <summary>The action parameter description</summary>
    public string Description { get; set; }

    /// <summary>The action parameter types</summary>
    public List<string> ValueTypes { get; set; }

    /// <summary>The action parameter source types</summary>
    public List<string> ValueSources { get; set; }

    /// <summary>The action parameter reference types</summary>
    public List<string> ValueReferences { get; set; }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ActionParameterInfo compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc />
    public override string ToString() => Name;
}