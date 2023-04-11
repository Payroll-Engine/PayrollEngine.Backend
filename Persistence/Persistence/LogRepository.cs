using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LogRepository : ChildDomainRepository<Domain.Model.Log>, ILogRepository
{
    public LogRepository(IDbContext context) :
        base(DbSchema.Tables.Log, DbSchema.LogColumn.TenantId, context)
    {
    }

    protected override void GetObjectData(Domain.Model.Log log, DbParameterCollection parameters)
    {
        parameters.Add(nameof(log.Level), log.Level);
        parameters.Add(nameof(log.Message), log.Message);
        parameters.Add(nameof(log.User), log.User);
        parameters.Add(nameof(log.Comment), log.Comment);
        parameters.Add(nameof(log.Owner), log.Owner);
        parameters.Add(nameof(log.OwnerType), log.OwnerType);
        base.GetObjectData(log, parameters);
    }
}