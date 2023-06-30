using System;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Webhook runtime message
/// </summary>
public class WebhookRuntimeMessage : WebhookMessage, IEquatable<WebhookRuntimeMessage>
{
    /// <summary>
    /// The tenant identifier
    /// </summary>
    public string Tenant { get; set; }

    /// <summary>
    /// The user identifier
    /// </summary>
    public string User { get; set; }

    /// <inheritdoc/>
    public WebhookRuntimeMessage()
    {
    }

    /// <summary>
    /// Webhook message Copy constructor
    /// </summary>
    /// <param name="copySource">The source to copy</param>
    /// <param name="tenant">The tenant identifier</param>
    /// <param name="user">The user identifier</param>
    public WebhookRuntimeMessage(WebhookMessage copySource, string tenant, string user) :
        base(copySource)
    {
        if (string.IsNullOrWhiteSpace(tenant))
        {
            throw new ArgumentException(nameof(tenant));
        }
        if (string.IsNullOrWhiteSpace(user))
        {
            throw new ArgumentException(nameof(user));
        }

        Tenant = tenant;
        User = user;
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="copySource">The source to copy</param>
    public WebhookRuntimeMessage(WebhookRuntimeMessage copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(WebhookRuntimeMessage compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Tenant} {User}: {base.ToString()}";
}