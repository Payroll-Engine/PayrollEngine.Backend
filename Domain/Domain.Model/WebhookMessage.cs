using System;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Webhook message
/// </summary>
public class WebhookMessage : DomainObjectBase, IEquatable<WebhookMessage>
{
    /// <summary>
    /// The webhook action name
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// The webhook receiver address
    /// </summary>
    public string ReceiverAddress { get; set; }

    /// <summary>
    /// The request date
    /// </summary>
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
    public WebhookMessage()
    {
    }

    /// <inheritdoc/>
    protected WebhookMessage(WebhookMessage copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(WebhookMessage compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ActionName} {RequestDate}: {ResponseStatus} {base.ToString()}";
}