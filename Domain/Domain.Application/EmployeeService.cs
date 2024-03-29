﻿using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeService
    (IEmployeeRepository repository) : ChildApplicationService<IEmployeeRepository, Employee>(repository),
        IEmployeeService
{
    public async Task<bool> ExistsAnyAsync(IDbContext context, int tenantId, string identifier) =>
        await Repository.ExistsAnyAsync(context, tenantId, identifier);
}