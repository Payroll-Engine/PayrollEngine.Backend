using System.Data;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LogRepository
    () : ChildDomainRepository<Domain.Model.Log>(DbSchema.Tables.Log, DbSchema.LogColumn.TenantId), ILogRepository
{
    protected override void GetObjectData(Domain.Model.Log log, DbParameterCollection parameters)
    {
        parameters.Add(nameof(log.Level), log.Level, DbType.Int32);
        parameters.Add(nameof(log.Message), log.Message);
        parameters.Add(nameof(log.User), log.User);
        parameters.Add(nameof(log.Comment), log.Comment);
        parameters.Add(nameof(log.Owner), log.Owner);
        parameters.Add(nameof(log.OwnerType), log.OwnerType);
        base.GetObjectData(log, parameters);
    }
}