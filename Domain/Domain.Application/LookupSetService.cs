﻿using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class LookupSetService : ChildApplicationService<ILookupSetRepository, LookupSet>, ILookupSetService
{
    public LookupSetService(ILookupSetRepository repository) :
        base(repository)
    {
    }

    public async Task<LookupSet> GetSetAsync(int tenantId, int regulationId, int lookupId) =>
        await Repository.GetSetAsync(tenantId, regulationId, lookupId);
}