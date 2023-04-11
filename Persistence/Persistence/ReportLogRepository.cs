using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportLogRepository : ChildDomainRepository<ReportLog>, IReportLogRepository
{
    public ReportLogRepository(IDbContext context) :
        base(DbSchema.Tables.ReportLog, DbSchema.ReportLogColumn.TenantId, context)
    {
    }

    protected override void GetObjectCreateData(ReportLog log, DbParameterCollection parameters)
    {
        parameters.Add(nameof(log.ReportName), log.ReportName);
        parameters.Add(nameof(log.ReportDate), log.ReportDate);
        parameters.Add(nameof(log.Key), log.Key);
        parameters.Add(nameof(log.User), log.User);
        parameters.Add(nameof(log.Message), log.Message);
        base.GetObjectCreateData(log, parameters);
    }
}