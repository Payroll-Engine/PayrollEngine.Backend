using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The lookup settings
/// </summary>
public class LookupSettings
{
    /// <summary>
    /// The lookup name
    /// </summary>
    [Required]
    public string LookupName { get; set; }

    /// <summary>
    /// The lookup value field name
    /// </summary>
    public string ValueFieldName { get; set; }

    /// <summary>
    /// The lookup text/display field name
    /// </summary>
    public string TextFieldName { get; set; }

    /// <inheritdoc />
    public override string ToString() => LookupName;
}