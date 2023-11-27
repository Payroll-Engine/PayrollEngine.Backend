using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class LookupValueRepository(ILookupValueAuditRepository auditRepository) :
    TrackChildDomainRepository<LookupValue, LookupValueAudit>(DbSchema.Tables.LookupValue,
        DbSchema.LookupValueColumn.LookupId, auditRepository), ILookupValueRepository
{
    public async Task<bool> ExistsAsync(IDbContext context, int lookupId, string key, decimal? rangeValue = null)
    {
        var conditions = new Dictionary<string, object>
        {
            { DbSchema.LookupValueColumn.LookupId, lookupId },
            { DbSchema.LookupValueColumn.LookupHash, key.ToPayrollHash(rangeValue) }
        };
        return (await SelectAsync<LookupValue>(context, TableName, conditions)).Any();
    }

    public async Task<int> DeleteAll(IDbContext context, int lookupId)
    {
        var query = DbQueryFactory.NewDeleteQuery(TableName, ParentFieldName, lookupId);
        var compileQuery = CompileQuery(query);
        var deleted = await ExecuteAsync(context, compileQuery);
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
        parameters.Add(nameof(value.OverrideType), value.OverrideType);
        base.GetObjectData(value, parameters);
    }
}