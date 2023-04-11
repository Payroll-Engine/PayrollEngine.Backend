using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Object containing settings
/// </summary>
public interface IDomainObject : IEquatable<IDomainObject>
{
    /// <summary>
    /// Unique object id
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// The object status: active (default) or inactive
    /// </summary>
    ObjectStatus Status { get; set; }

    /// <summary>
    /// The date of the domain object creation
    /// </summary>
    DateTime Created { get; set; }

    /// <summary>
    /// The date which the domain object was last updated
    /// </summary>
    DateTime Updated { get; set; }
}