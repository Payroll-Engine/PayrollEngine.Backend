using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case field reference
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CaseFieldReference : IEquatable<CaseFieldReference>
{
    /// <summary>
    /// The case field name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The case field order
    /// </summary>
    public int? Order { get; set; }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseFieldReference compare) =>
        CompareTool.EqualProperties(this, compare);
}