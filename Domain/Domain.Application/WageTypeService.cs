using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class WageTypeService : ScriptTrackChildApplicationService<IWageTypeRepository, WageType, WageTypeAudit>, IWageTypeService
{
    public WageTypeService(IWageTypeRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAnyAsync(int regulationId, IEnumerable<decimal> wageTypeNumbers) =>
        await Repository.ExistsAnyAsync(regulationId, wageTypeNumbers);
}