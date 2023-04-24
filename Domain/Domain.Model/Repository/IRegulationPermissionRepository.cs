namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for regulation permissions
/// </summary>
public interface IRegulationPermissionRepository : IRootDomainRepository<RegulationPermission>
{
    /// <summary>
    /// Test for permission
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="permissionTenantId">The permission tenant id</param>
    /// <param name="permissionDivisionId">The permission division id</param>
    /// <returns>True any collector exists</returns>
    System.Threading.Tasks.Task<RegulationPermission> GetAsync(IDbContext context, int tenantId, 
        int regulationId, int permissionTenantId, int? permissionDivisionId);
}