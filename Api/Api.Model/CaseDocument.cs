using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The case document API object
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CaseDocument : ApiObjectBase
{
    /// <summary>
    /// The document name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// The document content
    /// </summary>
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// The document content type
    /// </summary>
    [Required]
    [StringLength(128)]
    public string ContentType { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}