﻿using System.Collections.Generic;
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

    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<decimal> wageTypeNumbers) =>
        await Repository.ExistsAnyAsync(context, regulationId, wageTypeNumbers);
}