using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class CaseFieldAuditRepository : AuditChildDomainRepository<CaseFieldAudit>, ICaseFieldAuditRepository
{
    public CaseFieldAuditRepository(IDbContext context) :
        base(DbSchema.Tables.CaseFieldAudit, DbSchema.CaseFieldAuditColumn.CaseFieldId, context)
    {
    }

    protected override void GetObjectCreateData(CaseFieldAudit audit, DbParameterCollection parameters)
    {
        // local fields
        // all audit fields
        // keep in sync with object properties
        parameters.Add(nameof(audit.CaseFieldId), audit.CaseFieldId);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.Tags), JsonSerializer.SerializeList(audit.Tags));
        parameters.Add(nameof(audit.ValueType), audit.ValueType);
        parameters.Add(nameof(audit.ValueScope), audit.ValueScope);
        parameters.Add(nameof(audit.TimeType), audit.TimeType);
        parameters.Add(nameof(audit.TimeUnit), audit.TimeUnit);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType);
        parameters.Add(nameof(audit.CancellationMode), audit.CancellationMode);
        parameters.Add(nameof(audit.ValueCreationMode), audit.ValueCreationMode);
        parameters.Add(nameof(audit.Optional), audit.Optional.ToDbValue());
        parameters.Add(nameof(audit.Order), audit.Order);
        parameters.Add(nameof(audit.StartDateType), audit.StartDateType);
        parameters.Add(nameof(audit.EndDateType), audit.EndDateType);
        parameters.Add(nameof(audit.EndMandatory), audit.EndMandatory);
        parameters.Add(nameof(audit.DefaultStart), audit.DefaultStart);
        parameters.Add(nameof(audit.DefaultEnd), audit.DefaultEnd);
        parameters.Add(nameof(audit.DefaultValue), audit.DefaultValue);
        parameters.Add(nameof(audit.LookupSettings), audit.LookupSettings);
        parameters.Add(nameof(audit.Clusters), JsonSerializer.SerializeList(audit.Clusters));
        parameters.Add(nameof(audit.BuildActions), JsonSerializer.SerializeList(audit.BuildActions));
        parameters.Add(nameof(audit.ValidateActions), JsonSerializer.SerializeList(audit.ValidateActions));
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        parameters.Add(nameof(audit.ValueAttributes), JsonSerializer.SerializeNamedDictionary(audit.ValueAttributes));
      
        // base fields
        base.GetObjectCreateData(audit, parameters);
    }
}