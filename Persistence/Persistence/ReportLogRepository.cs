using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportLogRepository() : ChildDomainRepository<ReportLog>(DbSchema.Tables.ReportLog,
    DbSchema.ReportLogColumn.TenantId), IReportLogRepository
{
    protected override void GetObjectCreateData(ReportLog log, DbParameterCollection parameters)
    {
        parameters.Add(nameof(log.ReportName), log.ReportName);
        parameters.Add(nameof(log.ReportDate), log.ReportDate, DbType.DateTime2);
        parameters.Add(nameof(log.Key), log.Key);
        parameters.Add(nameof(log.User), log.User);
        parameters.Add(nameof(log.Message), log.Message);
        base.GetObjectCreateData(log, parameters);
    }
}