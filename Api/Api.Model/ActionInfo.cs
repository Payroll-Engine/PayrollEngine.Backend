using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PayrollEngine.Action;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Action info
/// </summary>
public class ActionInfo
{
    /// <summary>
    /// The action name
    /// </summary>
    [Required]
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
}