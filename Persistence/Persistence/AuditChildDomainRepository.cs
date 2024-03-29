﻿using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class AuditChildDomainRepository<T>
    (string tableName, string parentFieldName) : ChildDomainRepository<T>(tableName, parentFieldName),
        IAuditChildDomainRepository<T>
    where T : AuditDomainObject
{
    public virtual async Task<T> GetCurrentAuditAsync(IDbContext context, int trackObjectId)
    {
        // query: last created audit before the tracking object
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, trackObjectId);
        // query compilation
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var audit = (await QueryAsync<T>(context, compileQuery)).MaxBy(x => x.Created);

        // notification
        if (audit != null)
        {
            await OnRetrieved(context, trackObjectId, audit);
        }

        return audit;
    }

    public virtual async Task<T> GetAuditAtAsync(IDbContext context, int trackObjectId, DateTime moment)
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
        var audits = (await QueryAsync<T>(context, compileQuery)).ToList();
        if (audits.Count != 1)
        {
            return null;
        }

        // notification
        var audit = audits[0];
        await OnRetrieved(context, trackObjectId, audit);

        return audit;
    }
}