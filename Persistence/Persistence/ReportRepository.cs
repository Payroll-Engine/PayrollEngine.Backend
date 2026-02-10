using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportRepository(IRegulationRepository regulationRepository, IScriptRepository scriptRepository,
    IReportAuditRepository auditRepository, bool auditEnabled)
    : ReportRepositoryBase<Report>(regulationRepository, scriptRepository, auditRepository, auditEnabled),
        IReportRepository;
