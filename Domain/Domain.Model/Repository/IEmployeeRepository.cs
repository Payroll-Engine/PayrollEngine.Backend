﻿using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for tenants
/// </summary>
public interface IEmployeeRepository : IChildDomainRepository<Employee>
{
    /// <summary>
    /// Determine if the employee existing by the identifier
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="identifier">The user identifier</param>
    /// <returns>True if the employee with this identifier exists</returns>
    Task<bool> ExistsAnyAsync(int tenantId, string identifier);
}