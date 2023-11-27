using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportRepository(IScriptRepository scriptRepository,
        IReportAuditRepository auditRepository)
    : ReportRepositoryBase<Report>(scriptRepository, auditRepository), IReportRepository;