using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class WageTypeAuditRepository() : AuditChildDomainRepository<WageTypeAudit>(DbSchema.Tables.WageTypeAudit,
    DbSchema.WageTypeAuditColumn.WageTypeId), IWageTypeAuditRepository
{
    protected override void GetObjectCreateData(WageTypeAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.WageTypeId), audit.WageTypeId, DbType.Int32);
        parameters.Add(nameof(audit.WageTypeNumber), audit.WageTypeNumber, DbType.Decimal);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        parameters.Add(nameof(audit.ValueType), audit.ValueType, DbType.Int32);
        parameters.Add(nameof(audit.Calendar), audit.Calendar);
        parameters.Add(nameof(audit.Culture), audit.Culture);
        parameters.Add(nameof(audit.Collectors), JsonSerializer.SerializeList(audit.Collectors));
        parameters.Add(nameof(audit.CollectorGroups), JsonSerializer.SerializeList(audit.CollectorGroups));
        parameters.Add(nameof(audit.ValueExpression), audit.ValueExpression);
        parameters.Add(nameof(audit.ResultExpression), audit.ResultExpression);
        parameters.Add(nameof(audit.ValueActions), JsonSerializer.SerializeList(audit.ValueActions));
        parameters.Add(nameof(audit.ResultActions), JsonSerializer.SerializeList(audit.ResultActions));
        parameters.Add(nameof(audit.Script), audit.Script);
        parameters.Add(nameof(audit.ScriptVersion), audit.ScriptVersion);
        parameters.Add(nameof(audit.Binary), audit.Binary, DbType.Binary);
        parameters.Add(nameof(audit.ScriptHash), audit.ScriptHash, DbType.Int32);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        parameters.Add(nameof(audit.Clusters), JsonSerializer.SerializeList(audit.Clusters));
        base.GetObjectCreateData(audit, parameters);
    }
}