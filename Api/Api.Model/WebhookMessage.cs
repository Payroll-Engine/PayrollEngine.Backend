using System;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The Webhook message API object
/// </summary>
public class WebhookMessage : ApiObjectBase
{
    /// <summary>
    /// The webhook action name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string ActionName { get; set; }

    /// <summary>
    /// The webhook receiver address
    /// </summary>
    [Required]
    [StringLength(128)]
    public string ReceiverAddress { get; set; }

    /// <summary>
    /// The request date
    /// </summary>
    [Required]
    public DateTime RequestDate { get; set; }

    /// <summary>
    /// The request message
    /// </summary>
    public string RequestMessage { get; set; }

    /// <summary>
    /// The request operation
    /// </summary>
    public string RequestOperation { get; set; }

    /// <summary>
    /// The response date
    /// </summary>
    public DateTime ResponseDate { get; set; }

    /// <summary>
    /// The response HTTP status code
    /// </summary>
    public int ResponseStatus { get; set; }

    /// <summary>
    /// The response message
    /// </summary>
    public string ResponseMessage { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ActionName} {base.ToString()}";
}