using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class CaseAuditRepository() : AuditChildDomainRepository<CaseAudit>(DbSchema.Tables.CaseAudit,
    DbSchema.CaseAuditColumn.CaseId), ICaseAuditRepository
{
    protected override void GetObjectCreateData(CaseAudit audit, DbParameterCollection parameters)
    {
        // local fields
        // all audit fields
        // keep in sync with object properties
        parameters.Add(nameof(audit.CaseId), audit.CaseId, DbType.Int32);
        parameters.Add(nameof(audit.CaseType), audit.CaseType, DbType.Int32);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.NameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.NameLocalizations));
        parameters.Add(nameof(audit.NameSynonyms), JsonSerializer.SerializeList(audit.NameSynonyms));
        parameters.Add(nameof(audit.Description), audit.Description);
        parameters.Add(nameof(audit.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DescriptionLocalizations));
        parameters.Add(nameof(audit.DefaultReason), audit.DefaultReason);
        parameters.Add(nameof(audit.DefaultReasonLocalizations), JsonSerializer.SerializeNamedDictionary(audit.DefaultReasonLocalizations));
        parameters.Add(nameof(audit.BaseCase), audit.BaseCase);
        parameters.Add(nameof(audit.BaseCaseFields), JsonSerializer.SerializeList(audit.BaseCaseFields));
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        parameters.Add(nameof(audit.CancellationType), audit.CancellationType, DbType.Int32);
        parameters.Add(nameof(audit.AvailableExpression), audit.AvailableExpression);
        parameters.Add(nameof(audit.Hidden), audit.Hidden, DbType.Boolean);
        parameters.Add(nameof(audit.BuildExpression), audit.BuildExpression);
        parameters.Add(nameof(audit.ValidateExpression), audit.ValidateExpression);
        parameters.Add(nameof(audit.Lookups), JsonSerializer.SerializeList(audit.Lookups));
        parameters.Add(nameof(audit.Slots), DefaultJsonSerializer.Serialize(audit.Slots));
        parameters.Add(nameof(audit.Script), audit.Script);
        parameters.Add(nameof(audit.ScriptVersion), audit.ScriptVersion);
        parameters.Add(nameof(audit.Binary), audit.Binary, DbType.Binary);
        parameters.Add(nameof(audit.ScriptHash), audit.ScriptHash, DbType.Int32);
        parameters.Add(nameof(audit.AvailableActions), JsonSerializer.SerializeList(audit.AvailableActions));
        parameters.Add(nameof(audit.BuildActions), JsonSerializer.SerializeList(audit.BuildActions));
        parameters.Add(nameof(audit.ValidateActions), JsonSerializer.SerializeList(audit.ValidateActions));
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
        parameters.Add(nameof(audit.Clusters), JsonSerializer.SerializeList(audit.Clusters));

        // base fields
        base.GetObjectCreateData(audit, parameters);
    }
}