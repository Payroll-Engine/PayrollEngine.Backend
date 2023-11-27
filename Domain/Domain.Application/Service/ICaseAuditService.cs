using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface ICaseAuditService : IChildApplicationService<ICaseAuditRepository, CaseAudit>;