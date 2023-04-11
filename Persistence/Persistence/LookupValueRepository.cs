using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class LookupValueRepository : TrackChildDomainRepository<LookupValue, LookupValueAudit>, ILookupValueRepository
{
    public LookupValueRepository(ILookupValueAuditRepository auditRepository, IDbContext context) :
        base(DbSchema.Tables.LookupValue, DbSchema.LookupValueColumn.LookupId, auditRepository, context)
    {
    }

    public virtual async Task<bool> ExistsAsync(int lookupId, string key, decimal? rangeValue = null)
    {
        var conditions = new Dictionary<string, object>
        {
            { DbSchema.LookupValueColumn.LookupId, lookupId },
            { DbSchema.LookupValueColumn.LookupHash, key.ToPayrollHash(rangeValue) }
        };
        return (await SelectAsync<LookupValue>(TableName, conditions)).Any();
    }

    public virtual async Task<int> DeleteAll(int lookupId)
    {
        var query = DbQueryFactory.NewDeleteQuery(TableName, ParentFieldName, lookupId);
        var compileQuery = CompileQuery(query);
        var deleted = await ExecuteAsync(compileQuery);
        return deleted;
    }

    protected override void GetObjectData(LookupValue value, DbParameterCollection parameters)
    {
        parameters.Add(nameof(value.Key), value.Key);
        parameters.Add(nameof(value.KeyHash), value.KeyHash);
        parameters.Add(nameof(value.RangeValue), value.RangeValue);
        parameters.Add(nameof(value.Value), value.Value);
        parameters.Add(nameof(value.ValueLocalizations), JsonSerializer.SerializeNamedDictionary(value.ValueLocalizations));
        parameters.Add(nameof(value.LookupHash), value.LookupHash);
        base.GetObjectData(value, parameters);
    }
}