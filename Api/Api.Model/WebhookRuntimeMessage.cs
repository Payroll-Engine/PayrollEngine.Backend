using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The Webhook runtime message API object
/// </summary>
public class WebhookRuntimeMessage : WebhookMessage
{
    /// <summary>
    /// The tenant identifier
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Tenant { get; set; }

    /// <summary>
    /// The user identifier
    /// </summary>
    [Required]
    [StringLength(128)]
    public string User { get; set; }
}