using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a domain object, containing an id and an object status
/// </summary>
public abstract class DomainObjectBase : IDomainObject
{
    /// <summary>
    /// The unique object id (immutable)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The object status: active (default) or inactive
    /// </summary>
    public ObjectStatus Status { get; set; }

    /// <summary>
    /// The date of the domain object creation
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The date which the domain object was last updated
    /// </summary>
    public DateTime Updated { get; set; }

    /// <summary>
    /// default constructor
    /// </summary>
    protected DomainObjectBase()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="copySource">The source to copy</param>
    protected DomainObjectBase(IDomainObject copySource)
    {
        if (copySource == null)
        {
            throw new ArgumentNullException(nameof(copySource));
        }

        Id = copySource.Id;
        Status = copySource.Status;
        Created = copySource.Created;
        Updated = copySource.Updated;
    }

    /// <inheritdoc/>
    public override string ToString() => ToObjectString();

    /// <summary>Get object string</summary>
    /// <returns>The object string</returns>
    protected string ToObjectString()
    {
        var inactiveMarker = Status == ObjectStatus.Inactive ? " [I]" : string.Empty;
        return $"#{Id}{inactiveMarker}";
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(IDomainObject compare)
    {
        if (ReferenceEquals(null, compare))
        {
            return false;
        }
        if (ReferenceEquals(this, compare))
        {
            return true;
        }
        return Id == compare.Id &&
               Status == compare.Status &&
               Created.Equals(compare.Created) &&
               Updated.Equals(compare.Updated);
    }
}