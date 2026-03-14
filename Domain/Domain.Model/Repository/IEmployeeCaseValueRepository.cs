using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for employee case values
/// </summary>
public interface IEmployeeCaseValueRepository : ICaseValueRepository
{
    /// <summary>
    /// Get case values for all active employees of a tenant at a specific point in time.
    /// Returns one entry per active case value per employee, enriched with EmployeeId.
    /// Uses a single JOIN query (GetEmployeeCaseValuesByTenant SP) instead of N+1 per-employee calls.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="valueDate">The value date — only values active at this date (Start &lt;= valueDate &lt; End)</param>
    /// <param name="evaluationDate">The evaluation date — only values created on or before this date</param>
    /// <param name="caseFieldNames">Case field name filter (null = all fields)</param>
    /// <param name="forecast">Forecast name (null = real values only)</param>
    /// <returns>Case values for all active employees, each with EmployeeId set</returns>
    Task<IEnumerable<CaseValue>> GetTenantCaseValuesAsync(IDbContext context, int tenantId,
        DateTime? valueDate = null, DateTime? evaluationDate = null,
        IEnumerable<string> caseFieldNames = null, string forecast = null);
}
