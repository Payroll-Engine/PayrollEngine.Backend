using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A webhook
/// </summary>
public class Webhook : DomainObjectBase, INamedObject, IDomainAttributeObject, IEquatable<Webhook>
{
    /// <summary>
    /// The webhook name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The webhook receiver address
    /// </summary>
    public string ReceiverAddress { get; set; }

    /// <summary>
    /// The webhook action
    /// </summary>
    public WebhookAction Action { get; set; }

    /// <summary>
    /// The signing secret.
    /// Write: the plain secret, the application service stores it protected.
    /// Read: the protected secret (never the plain text).
    /// On update: null keeps the stored secret, an empty value removes it, any other value replaces it.
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    /// Indicates whether the webhook messages are signed (a secret is stored)
    /// </summary>
    public bool Signed => !string.IsNullOrEmpty(Secret);

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public Webhook()
    {
    }

    /// <inheritdoc/>
    public Webhook(Webhook copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Webhook compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {Action} {base.ToString()}";
}