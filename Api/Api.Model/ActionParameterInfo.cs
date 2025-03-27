using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Action parameter info
/// </summary>
public class ActionParameterInfo
{
    /// <summary>The action parameter name</summary>
    [Required]
    public string Name { get; set; }

    /// <summary>The action parameter description</summary>
    public string Description { get; set; }

    /// <summary>The action parameter types</summary>
    public List<string> ValueTypes { get; set; }

    /// <summary>The action parameter source types</summary>
    public List<string> ValueSources { get; set; }

    /// <summary>The action parameter reference types</summary>
    public List<string> ValueReferences { get; set; }
}