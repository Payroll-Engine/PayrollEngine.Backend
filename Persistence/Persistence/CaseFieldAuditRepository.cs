using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CaseFieldAuditRepository() : AuditChildDomainRepository<CaseFieldAudit>(DbSchema.Tables.CaseFieldAudit,
    DbSchema.CaseFieldAuditColumn.CaseFieldId), ICaseFieldAuditRepository
{
    protected override void GetObjectCreateData(CaseFieldAudit audit, DbParameterCollection parameters)
    {
        // local fields
        // all audit fields
        // keep in sync with object properties
        parameters.Add(nameof(audit.CaseFieldId), audit.CaseFieldId, DbType.Int32);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.Tags), JsonSerializer.SerializeList(audit.Tags));
        parameters.Add(nameof(audit.ValueType), audit.ValueType, DbType.Int32);
        parameters.Add(nameof(audit.ValueScope), audit.ValueScope, DbType.Int32);
        parameters.Add(nameof(audit.TimeType), audit.TimeType, DbType.Int32);
        parameters.Add(nameof(audit.TimeUnit), audit.TimeUnit, DbType.Int32);
        parameters.Add(nameof(audit.PeriodAggregation), audit.PeriodAggregation, DbType.Int32);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        parameters.Add(nameof(audit.CancellationMode), audit.CancellationMode, DbType.Int32);
        parameters.Add(nameof(audit.ValueCreationMode), audit.ValueCreationMode, DbType.Int32);
        parameters.Add(nameof(audit.Culture), audit.Culture);
        parameters.Add(nameof(audit.ValueMandatory), audit.ValueMandatory, DbType.Boolean);
        parameters.Add(nameof(audit.Order), audit.Order, DbType.Int32);
        parameters.Add(nameof(audit.StartDateType), audit.StartDateType, DbType.Int32);
        parameters.Add(nameof(audit.EndDateType), audit.EndDateType, DbType.Int32);
        parameters.Add(nameof(audit.EndMandatory), audit.EndMandatory, DbType.Boolean);
        parameters.Add(nameof(audit.DefaultStart), audit.DefaultStart);
        parameters.Add(nameof(audit.DefaultEnd), audit.DefaultEnd);
        parameters.Add(nameof(audit.DefaultValue), audit.DefaultValue);
        parameters.Add(nameof(audit.LookupSettings), audit.LookupSettings);
        parameters.Add(nameof(audit.Clusters), JsonSerializer.SerializeList(audit.Clusters));
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        parameters.Add(nameof(audit.ValueAttributes), JsonSerializer.SerializeNamedDictionary(audit.ValueAttributes));
      
        // base fields
        base.GetObjectCreateData(audit, parameters);
    }
}