using System.Linq;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupValueRepository(IRegulationRepository regulationRepository,
    ILookupValueAuditRepository auditRepository, bool auditDisabled) :
    TrackChildDomainRepository<LookupValue, LookupValueAudit>(regulationRepository, DbSchema.Tables.LookupValue,
        DbSchema.LookupValueColumn.LookupId, auditRepository, auditDisabled), ILookupValueRepository
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
        parameters.Add(nameof(value.KeyHash), value.KeyHash, DbType.Int32);
        parameters.Add(nameof(value.RangeValue), value.RangeValue, DbType.Decimal);
        parameters.Add(nameof(value.Value), value.Value);
        parameters.Add(nameof(value.ValueLocalizations), JsonSerializer.SerializeNamedDictionary(value.ValueLocalizations));
        parameters.Add(nameof(value.LookupHash), value.LookupHash, DbType.Int32);
        parameters.Add(nameof(value.OverrideType), value.OverrideType, DbType.Int32);
        base.GetObjectData(value, parameters);
    }
}