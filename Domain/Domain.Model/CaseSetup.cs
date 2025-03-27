using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case setup
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public class CaseSetup : IEquatable<CaseSetup>
{
    /// <summary>
    /// The case name
    /// </summary>
    public string CaseName { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    public string CaseSlot { get; set; }

    /// <summary>
    /// The case value setups
    /// </summary>
    public List<CaseValueSetup> Values { get; set; } = [];

    /// <summary>
    /// The related cases
    /// </summary>
    public List<CaseSetup> RelatedCases { get; set; } = [];

    /// <summary>Initializes a new instance of the <see cref="CaseSetup"/> class</summary>
    public CaseSetup()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CaseSetup"/> class</summary>
    /// <param name="copySource">The copy source</param>
    public CaseSetup(CaseSetup copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseSetup compare) =>
        CompareTool.EqualProperties(this, compare);
}