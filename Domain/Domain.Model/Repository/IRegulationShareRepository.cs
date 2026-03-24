namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for regulation permissions
/// </summary>
public interface IRegulationShareRepository : IRootDomainRepository<RegulationShare>
{
    /// <summary>Test for permission</summary>
    /// <param name="context">The database context</param>
    /// <param name="providerTenantId">The tenant id</param>
    /// <param name="providerRegulationId">The regulation id</param>
    /// <param name="consumerTenantId">The permission tenant id</param>
    /// <param name="consumerDivisionId">The permission division id</param>
    System.Threading.Tasks.Task<RegulationShare> GetAsync(IDbContext context, int providerTenantId,
        int providerRegulationId, int consumerTenantId, int? consumerDivisionId);

    /// <summary>Query all regulation shares for a consumer tenant with at least the specified isolation level</summary>
    /// <param name="context">The database context</param>
    /// <param name="consumerTenantId">The consumer tenant id</param>
    /// <param name="minLevel">The minimum required isolation level</param>
    System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<RegulationShare>> GetConsumerSharesAsync(
        IDbContext context, int consumerTenantId, TenantIsolationLevel minLevel);

    /// <summary>Query regulation shares for a specific division of a consumer tenant with at least the specified isolation level</summary>
    /// <param name="context">The database context</param>
    /// <param name="consumerTenantId">The consumer tenant id</param>
    /// <param name="divisionId">The consumer division id</param>
    /// <param name="minLevel">The minimum required isolation level</param>
    System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<RegulationShare>> GetConsumerDivisionSharesAsync(
        IDbContext context, int consumerTenantId, int divisionId, TenantIsolationLevel minLevel);
}