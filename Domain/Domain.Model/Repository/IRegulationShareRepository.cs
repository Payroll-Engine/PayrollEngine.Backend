namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for regulation permissions
/// </summary>
public interface IRegulationShareRepository : IRootDomainRepository<RegulationShare>
{
    /// <summary>
    /// Test for permission
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="providerTenantId">The tenant id</param>
    /// <param name="providerRegulationId">The regulation id</param>
    /// <param name="consumerTenantId">The permission tenant id</param>
    /// <param name="consumerDivisionId">The permission division id</param>
    /// <returns>True any collector exists</returns>
    System.Threading.Tasks.Task<RegulationShare> GetAsync(IDbContext context, int providerTenantId, 
        int providerRegulationId, int consumerTenantId, int? consumerDivisionId);
}