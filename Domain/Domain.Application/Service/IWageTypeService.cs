using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IWageTypeService : IScriptTrackChildApplicationService<IWageTypeRepository, WageType, WageTypeAudit>
{
    /// <summary>
    /// Test if wage types exists
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <returns>True if the case exists</returns>
    Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<decimal> wageTypeNumbers);
}