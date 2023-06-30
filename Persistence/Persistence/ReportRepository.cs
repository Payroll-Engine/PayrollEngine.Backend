using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportRepository : ReportRepositoryBase<Report>, IReportRepository
{
    public ReportRepository(IScriptRepository scriptRepository,
        IReportAuditRepository auditRepository) :
        base(scriptRepository, auditRepository)
    {
    }
}