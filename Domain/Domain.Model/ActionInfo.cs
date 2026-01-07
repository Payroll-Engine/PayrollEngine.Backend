using System;
using System.Collections.Generic;
using PayrollEngine.Action;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Action info
/// </summary>
public class ActionInfo : IEquatable<ActionInfo>
{
    /// <summary>The action name</summary>
    public string Name { get; set; }

    /// <summary>The extension function type</summary>
    public FunctionType FunctionType { get; set; }

    /// <summary>The action namespace</summary>
    public string Namespace { get; set; }

    /// <summary>The action description</summary>
    public string Description { get; set; }

    /// <summary>The action categories</summary>
    public List<string> Categories { get; set; }

    /// <summary>Action source </summary>
    public ActionSource Source { get; set; }

    /// <summary>The action parameters</summary>
    public List<ActionParameterInfo> Parameters { get; set; }

    /// <summary>The action issues</summary>
    public List<ActionIssueInfo> Issues { get; set; }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ActionInfo compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc />
    public override string ToString() => Name;

}