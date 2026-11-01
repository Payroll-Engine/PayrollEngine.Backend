// ReSharper disable UnusedAutoPropertyAccessor.Global
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
    /// The secret used to sign the webhook messages (write-only, never returned by the API).
    /// On update: omitted keeps the current secret, an empty value removes it, any other value replaces it.
    /// </summary>
    [StringLength(256)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Secret { get; set; }

    /// <summary>
    /// Indicates whether the webhook messages are signed (a secret is set)
    /// </summary>
    public bool Signed { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string,object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}