using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for wage types
/// </summary>
public interface IWageTypeRepository : IScriptTrackDomainObjectRepository<WageType, WageTypeAudit>
{
    /// <summary>
    /// Determine if any of the wage type identifiers are existing
    /// </summary>
    /// <param name="regulationId">The payroll regulation id</param>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <returns>True if any wage type with this identifier exists</returns>
    Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<decimal> wageTypeNumbers);
}