using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ICaseChangeService<out TRepo, TDomain> : IChildApplicationService<TRepo, TDomain>
    where TRepo : class, IChildDomainRepository<TDomain>
    where TDomain : CaseChange, new()
{
    /// <summary>
    /// Query resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of the resources, matching the query</returns>
    Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Count query of resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Resource count matching the query</returns>
    Task<long> QueryCountAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Query case values of case changes
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">The query</param>
    /// <returns>A list of case values</returns>
    Task<IEnumerable<CaseChangeCaseValue>> QueryValuesAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Queries the values count
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="parentId">The parent identifier</param>
    /// <param name="query">The query</param>
    Task<long> QueryValuesCountAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Add payroll case values
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="change">The case values to add</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    Task<TDomain> AddCaseChangeAsync(IDbContext context, int tenantId, int userId, int payrollId, int parentId, TDomain change);
}