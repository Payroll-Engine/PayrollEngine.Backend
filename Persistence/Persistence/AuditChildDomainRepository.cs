using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class AuditChildDomainRepository<T> : ChildDomainRepository<T>, IAuditChildDomainRepository<T>
    where T : AuditDomainObject
{
    protected AuditChildDomainRepository(string tableName, string parentFieldName, IDbContext context) :
        base(tableName, parentFieldName, context)
    {
    }

    public virtual async Task<T> GetCurrentAuditAsync(int trackObjectId)
    {
        // query: last created audit before the tracking object
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, trackObjectId);
        // query compilation
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var audit = (await QueryAsync<T>(compileQuery)).MaxBy(x => x.Created);

        // notification
        if (audit != null)
        {
            await OnRetrieved(trackObjectId, audit);
        }

        return audit;
    }

    public virtual async Task<T> GetAuditAtAsync(int trackObjectId, DateTime moment)
    {
        // query: last created audit before the moment
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, trackObjectId);
        // TOP 1
        query.Limit(1);
        // exclude newer ones
        query.Where(DbSchema.ObjectColumn.Created, "<", moment);
        // take the newest audit at the first place
        query.OrderByDesc(DbSchema.ObjectColumn.Created);

        // query compilation
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var audits = (await QueryAsync<T>(compileQuery)).ToList();
        if (audits.Count != 1)
        {
            return null;
        }

        // notification
        var audit = audits[0];
        await OnRetrieved(trackObjectId, audit);

        return audit;
    }
}