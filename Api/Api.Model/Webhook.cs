using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The Webhook API object
/// </summary>
public class Webhook : ApiObjectBase
{
    /// <summary>
    /// The payroll name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The webhook receiver address
    /// </summary>
    [Required]
    [StringLength(128)]
    public string ReceiverAddress { get; set; }

    /// <summary>
    /// The web hook trigger action
    /// </summary>
    [Required]
    public WebhookAction Action { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string,object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}