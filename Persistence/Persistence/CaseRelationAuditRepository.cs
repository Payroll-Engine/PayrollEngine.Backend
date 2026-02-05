using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CaseRelationAuditRepository() : AuditChildDomainRepository<CaseRelationAudit>(
    DbSchema.Tables.CaseRelationAudit, DbSchema.CaseRelationAudit.CaseRelationId), ICaseRelationAuditRepository
{
    protected override void GetObjectCreateData(CaseRelationAudit audit, DbParameterCollection parameters)
    {
        // local fields
        // keep in sync with object properties
        parameters.Add(nameof(audit.SourceCaseName), audit.SourceCaseName);
        parameters.Add(nameof(audit.SourceCaseNameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.SourceCaseNameLocalizations));
        parameters.Add(nameof(audit.SourceCaseSlot), audit.SourceCaseSlot);
        parameters.Add(nameof(audit.SourceCaseSlotLocalizations), JsonSerializer.SerializeNamedDictionary(audit.SourceCaseSlotLocalizations));
        parameters.Add(nameof(audit.TargetCaseName), audit.TargetCaseName);
        parameters.Add(nameof(audit.TargetCaseNameLocalizations), JsonSerializer.SerializeNamedDictionary(audit.TargetCaseNameLocalizations));
        parameters.Add(nameof(audit.TargetCaseSlot), audit.TargetCaseSlot);
        parameters.Add(nameof(audit.TargetCaseSlotLocalizations), JsonSerializer.SerializeNamedDictionary(audit.TargetCaseSlotLocalizations));
        parameters.Add(nameof(audit.RelationHash), audit.RelationHash, DbType.Int32);
        parameters.Add(nameof(audit.BuildExpression), audit.BuildExpression);
        parameters.Add(nameof(audit.ValidateExpression), audit.ValidateExpression);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        parameters.Add(nameof(audit.Order), audit.Order, DbType.Int32);
        parameters.Add(nameof(audit.Script), audit.Script);
        parameters.Add(nameof(audit.ScriptVersion), audit.ScriptVersion);
        parameters.Add(nameof(audit.Binary), audit.Binary, DbType.Binary);
        parameters.Add(nameof(audit.ScriptHash), audit.ScriptHash, DbType.Int32);
        parameters.Add(nameof(audit.BuildActions), JsonSerializer.SerializeList(audit.BuildActions));
        parameters.Add(nameof(audit.ValidateActions), JsonSerializer.SerializeList(audit.ValidateActions));
        parameters.Add(nameof(audit.Attributes), JsonSerializer.SerializeNamedDictionary(audit.Attributes));
       
        // base fields
        base.GetObjectCreateData(audit, parameters);
    }
}