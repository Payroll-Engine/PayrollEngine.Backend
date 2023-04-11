using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting.Controller;

namespace PayrollEngine.Persistence;

public class ReportRepository : ReportRepositoryBase<Report>, IReportRepository
{
    public ReportRepository(IReportScriptController<Report> scriptController, IScriptRepository scriptRepository,
        IReportAuditRepository auditRepository, IDbContext context) :
        base(scriptController, scriptRepository, auditRepository, context)
    {
    }
}