using System;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic audit domain object repository with support of the basic CRUD operations
/// </summary>
/// <typeparam name="TDomain">The domain object with an audit</typeparam>
public interface IAuditChildDomainRepository<TDomain> : IChildDomainRepository<TDomain>
    where TDomain : AuditDomainObject
{
    /// <summary>
    /// Get current audit object from the tracked item
    /// </summary>
    /// <param name="trackObjectId">The tracking object id</param>
    /// <returns>The audit object</returns>
    Task<TDomain> GetCurrentAuditAsync(int trackObjectId);

    /// <summary>
    /// Get audit object from a given time, based on the modification date
    /// </summary>
    /// <param name="trackObjectId">The tracking object id</param>
    /// <param name="moment">The time moment</param>
    /// <returns>The domain object at a given time.
    /// For future dates the latest modified object will be used.
    /// A null result indicates that the object was not existing at the given time</returns>
    Task<TDomain> GetAuditAtAsync(int trackObjectId, DateTime moment);
}