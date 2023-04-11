﻿namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll case relation audits
/// </summary>
public interface ILookupAuditRepository : IAuditChildDomainRepository<LookupAudit>
{
}