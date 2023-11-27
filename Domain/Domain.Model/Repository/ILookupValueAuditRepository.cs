namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll case relation audits
/// </summary>
public interface ILookupValueAuditRepository : IAuditChildDomainRepository<LookupValueAudit>;