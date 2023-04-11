using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for <see cref="DomainObjectBase"/>
/// </summary>
public static class DomainObjectExtensions
{
    /// <summary>
    /// Check if the domain object has an id
    /// </summary>
    /// <param name="domainObject">The domain object</param>
    /// <returns>True, if the domain object contains an id</returns>
    public static bool HasId(this IDomainObject domainObject) => domainObject.Id > 0;

    /// <summary>
    /// Set object created and updated date
    /// </summary>
    /// <param name="domainObject">The domain object</param>
    /// <param name="created">The creation moment</param>
    public static void SetCreatedDate(this IDomainObject domainObject, DateTime created)
    {
        domainObject.Created = created;
        domainObject.Updated = created;
    }

    /// <summary>
    /// Set object created and updated date
    /// </summary>
    /// <param name="domainObject">The domain object</param>
    /// <param name="created">The creation moment</param>
    public static void InitCreatedDate(this IDomainObject domainObject, DateTime created)
    {
        if (domainObject.Created == Date.MinValue)
        {
            SetCreatedDate(domainObject, created);
        }
    }
}