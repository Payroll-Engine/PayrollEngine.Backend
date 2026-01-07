using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for <see cref="DomainObjectBase"/>
/// </summary>
public static class DomainObjectExtensions
{
    /// <param name="domainObject">The domain object</param>
    extension(IDomainObject domainObject)
    {
        /// <summary>
        /// Check if the domain object has an id
        /// </summary>
        /// <returns>True, if the domain object contains an id</returns>
        public bool HasId() => domainObject.Id > 0;

        /// <summary>
        /// Set object created and updated date
        /// </summary>
        /// <param name="created">The creation moment</param>
        public void SetCreatedDate(DateTime created)
        {
            domainObject.Created = created;
            domainObject.Updated = created;
        }

        /// <summary>
        /// Set object created and updated date
        /// </summary>
        /// <param name="created">The creation moment</param>
        public void InitCreatedDate(DateTime created)
        {
            if (domainObject.Created == Date.MinValue)
            {
                domainObject.SetCreatedDate(created);
            }
        }
    }
}