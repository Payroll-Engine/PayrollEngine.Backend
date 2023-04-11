using System;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The base API object
/// </summary>
public abstract class ApiObjectBase
{
    /// <summary>
    /// The unique object id (immutable)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The status of the object
    /// </summary>
    public ObjectStatus Status { get; set; }

    /// <summary>
    /// The date which the API object was created (immutable)
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The date which the API object was last updated (immutable)
    /// </summary>
    public DateTime Updated { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"#{Id}";
}