namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payruns
/// </summary>
public interface IPayrunRepository : IChildDomainRepository<Payrun>
{
    /// <summary>
    /// Rebuild the payrun
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunId">The payrun id</param>
    System.Threading.Tasks.Task RebuildAsync(IDbContext context, int tenantId, int payrunId);
}