using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WageTypeService(IWageTypeRepository repository) :
    ScriptTrackChildApplicationService<IWageTypeRepository, WageType, WageTypeAudit>(repository), IWageTypeService
{
    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<decimal> wageTypeNumbers) =>
        await Repository.ExistsAnyAsync(context, regulationId, wageTypeNumbers);
}