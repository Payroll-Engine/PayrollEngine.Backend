using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for case changes
/// </summary>
public interface ICaseChangeRepository<T> : IChildDomainRepository<T>
    where T : CaseChange
{
    /// <summary>
    /// Query resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of the resources, matching the query</returns>
    Task<IEnumerable<T>> QueryAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Count query of resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Resource count matching the query</returns>
    // ReSharper disable once UnusedParameter.Global
    Task<long> QueryCountAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Query case values of case changes
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of case values</returns>
    Task<IEnumerable<CaseChangeCaseValue>> QueryValuesAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Query count of case values of case changes
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Resource count matching the query</returns>
    Task<long> QueryValuesCountAsync(IDbContext context, int tenantId, int parentId, Query query = null);

    /// <summary>
    /// Add new case change
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="parentId">The change parent id</param>
    /// <param name="caseChange">The case values to add</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    Task<T> AddCaseChangeAsync(IDbContext context, int tenantId, int payrollId, int parentId, T caseChange);
}