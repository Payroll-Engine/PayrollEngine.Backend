using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case slot
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CaseSlot : IEquatable<CaseSlot>
{
    /// <summary>
    /// The case slot name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized case slot names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseSlot compare) =>
        CompareTool.EqualProperties(this, compare);
}