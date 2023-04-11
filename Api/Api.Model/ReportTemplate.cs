using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The report template API object
/// </summary>
public class ReportTemplate : ApiObjectBase
{
    /// <summary>
    /// The report language
    /// </summary>
    [Required]
    public Language Language { get; set; }

    /// <summary>
    /// The report content (client owned)
    /// </summary>
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// The report content type
    /// </summary>
    [StringLength(128)]
    public string ContentType { get; set; }
        
    /// <summary>
    /// The report schema (client owned)
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// The report external resource
    /// </summary>
    [StringLength(256)]
    public string Resource { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Language} {base.ToString()}";
}