using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportRepository(IRegulationRepository regulationRepository, IScriptRepository scriptRepository,
    IReportAuditRepository auditRepository, bool auditDisabled)
    : ReportRepositoryBase<Report>(regulationRepository, scriptRepository, auditRepository, auditDisabled),
        IReportRepository;
