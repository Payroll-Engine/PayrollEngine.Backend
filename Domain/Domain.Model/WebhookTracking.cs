namespace PayrollEngine.Domain.Model;

/// <summary>
/// Webhook message tracking: which dispatched webhook messages are persisted
/// together with the response of the receiver
/// </summary>
public enum WebhookTracking
{
    /// <summary>No tracking</summary>
    None,

    /// <summary>Track the notification webhooks (case change, payrun job, task)</summary>
    Notification,

    /// <summary>Track the notification webhooks and the scripting webhook calls (one message per call)</summary>
    All
}