// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Webhook dispatch message
/// </summary>
public class WebhookDispatchMessage
{
    /// <summary>
    /// The webhook action
    /// </summary>
    public WebhookAction Action { get; set; }

    /// <summary>
    /// The request message
    /// </summary>
    public string RequestMessage { get; set; }

    /// <summary>
    /// The request operation
    /// </summary>
    public string RequestOperation { get; set; }

    /// <summary>
    /// Webhook tracking option
    /// </summary>
    public bool TrackMessage { get; set; }
}