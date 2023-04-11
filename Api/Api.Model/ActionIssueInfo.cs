using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Action issue info
/// </summary>
public class ActionIssueInfo
{
    /// <summary>
    /// The action name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>The action issue message</summary>
    public string Message { get; set; }

    /// <summary>The action issue description</summary>
    public int ParameterCount { get; set; }
}