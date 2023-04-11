using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic domain object repository with support of the basic CRUD operations
/// </summary>
/// <typeparam name="TDomain">The domain object with an audit</typeparam>
/// <typeparam name="TAudit">The audit object</typeparam>
public interface ITrackChildDomainRepository<TDomain, TAudit> : IChildDomainRepository<TDomain>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    /// <summary>
    /// Get current audit object from the tracked item
    /// </summary>
    /// <param name="trackObjectId">The tracking object id</param>
    /// <returns>The audit object</returns>
    Task<TAudit> GetCurrentAuditAsync(int trackObjectId);

    /// <summary>
    /// Create object from audit object
    /// </summary>
    /// <param name="audit">The audit object</param>
    /// <returns>The domain object</returns>
    TDomain NewFromAudit(TAudit audit);
}