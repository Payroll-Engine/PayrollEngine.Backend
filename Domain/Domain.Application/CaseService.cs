﻿using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseService : ScriptTrackChildApplicationService<ICaseRepository, Case, CaseAudit>, ICaseService
{
    public CaseService(ICaseRepository repository) :
        base(repository)
    {
    }

    public async Task<Case> GetAsync(IDbContext context, int tenantId, int regulationId, string name) =>
        (await Repository.QueryAsync(context, tenantId, name, regulationId)).FirstOrDefault();
}