using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportRepository(IScriptRepository scriptRepository,
        IReportAuditRepository auditRepository, bool auditDisabled)
    : ReportRepositoryBase<Report>(scriptRepository, auditRepository, auditDisabled), IReportRepository;