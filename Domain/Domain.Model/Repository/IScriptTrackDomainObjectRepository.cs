namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for script objects
/// </summary>
public interface IScriptTrackDomainObjectRepository<TDomain, TAudit> : ITrackChildDomainRepository<TDomain, TAudit>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    /// <summary>
    /// Rebuild the script object
    /// </summary>
    /// <param name="parentId">The parent object id</param>
    /// <param name="itemId">The items id to build</param>
    System.Threading.Tasks.Task RebuildAsync(int parentId, int itemId);
}