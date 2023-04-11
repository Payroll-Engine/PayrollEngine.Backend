using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for lookups
/// </summary>
public interface ILookupRepository : ILookupRepository<Lookup>
{
}

/// <summary>
/// Repository for lookups
/// </summary>
public interface ILookupRepository<T> : ITrackChildDomainRepository<T, LookupAudit>
    where T : Lookup, new()
{
    /// <summary>
    /// Determine if any of the lookup names are existing
    /// </summary>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupNames">The lookup names</param>
    /// <returns>True if any lookup with this name exists</returns>
    Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<string> lookupNames);
}