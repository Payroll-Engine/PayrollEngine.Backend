using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Case setup
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CaseSetup : ApiObjectBase
{
    /// <summary>
    /// The case name
    /// </summary>
    [Required]
    public string CaseName { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    public string CaseSlot { get; set; }

    /// <summary>
    /// The case value setups
    /// </summary>
    public List<CaseValueSetup> Values { get; set; }

    /// <summary>
    /// The related cases
    /// </summary>
    public List<CaseSetup> RelatedCases { get; set; }
}