using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class WageTypeAuditRepository : AuditChildDomainRepository<WageTypeAudit>, IWageTypeAuditRepository
{
    public WageTypeAuditRepository() :
        base(DbSchema.Tables.WageTypeAudit, DbSchema.WageTypeAuditColumn.WageTypeId)
    {
    }

    protected override void GetObjectCreateData(WageTypeAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.WageTypeId), audit.WageTypeId);
        parameters.Add(nameof(audit.WageTypeNumber), audit.WageTypeNumber);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType);
        parameters.Add(nameof(audit.ValueType), audit.ValueType);
        parameters.Add(nameof(audit.CalendarCalculationMode), audit.CalendarCalculationMode);
        parameters.Add(nameof(audit.Collectors), JsonSerializer.SerializeList(audit.Collectors));
        parameters.Add(nameof(audit.CollectorGroups), JsonSerializer.SerializeList(audit.CollectorGroups));
        parameters.Add(nameof(audit.ValueExpression), audit.ValueExpression);
        parameters.Add(nameof(audit.ResultExpression), audit.ResultExpression);
        parameters.Add(nameof(audit.Script), audit.Script);
        parameters.Add(nameof(audit.ScriptVersion), audit.ScriptVersion);
        parameters.Add(nameof(audit.Binary), audit.Binary, DbType.Binary);
        parameters.Add(nameof(audit.ScriptHash), audit.ScriptHash);
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        parameters.Add(nameof(audit.Clusters), JsonSerializer.SerializeList(audit.Clusters));
        base.GetObjectCreateData(audit, parameters);
    }
}