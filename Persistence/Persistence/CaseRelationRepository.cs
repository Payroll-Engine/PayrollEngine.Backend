using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CaseRelationRepository(IRegulationRepository regulationRepository,
    IScriptRepository scriptRepository, ICaseRelationAuditRepository auditRepository, bool auditDisabled)
    : ScriptTrackChildDomainRepository<CaseRelation, CaseRelationAudit>(DbSchema.Tables.CaseRelation,
        DbSchema.CaseRelationColumn.RegulationId, regulationRepository, scriptRepository, auditRepository, auditDisabled), ICaseRelationRepository
{
    protected override void GetObjectCreateData(CaseRelation relation, DbParameterCollection parameters)
    {
        parameters.Add(nameof(relation.SourceCaseName), relation.SourceCaseName);
        parameters.Add(nameof(relation.SourceCaseNameLocalizations), JsonSerializer.SerializeNamedDictionary(relation.SourceCaseNameLocalizations));
        parameters.Add(nameof(relation.TargetCaseName), relation.TargetCaseName);
        parameters.Add(nameof(relation.TargetCaseNameLocalizations), JsonSerializer.SerializeNamedDictionary(relation.TargetCaseNameLocalizations));
        base.GetObjectCreateData(relation, parameters);
    }

    protected override void GetObjectData(CaseRelation relation, DbParameterCollection parameters)
    {
        parameters.Add(nameof(relation.SourceCaseSlot), relation.SourceCaseSlot);
        parameters.Add(nameof(relation.SourceCaseSlotLocalizations), JsonSerializer.SerializeNamedDictionary(relation.SourceCaseSlotLocalizations));
        parameters.Add(nameof(relation.TargetCaseSlot), relation.TargetCaseSlot);
        parameters.Add(nameof(relation.TargetCaseSlotLocalizations), JsonSerializer.SerializeNamedDictionary(relation.TargetCaseSlotLocalizations));
        parameters.Add(nameof(relation.RelationHash), relation.RelationHash, DbType.Int32);
        parameters.Add(nameof(relation.BuildExpression), relation.BuildExpression);
        parameters.Add(nameof(relation.ValidateExpression), relation.ValidateExpression);
        parameters.Add(nameof(relation.OverrideType), relation.OverrideType, DbType.Int32);
        parameters.Add(nameof(relation.Order), relation.Order, DbType.Int32);
        parameters.Add(nameof(relation.Script), relation.Script);
        parameters.Add(nameof(relation.ScriptVersion), relation.ScriptVersion);
        parameters.Add(nameof(relation.Binary), relation.Binary, DbType.Binary);
        parameters.Add(nameof(relation.ScriptHash), relation.ScriptHash, DbType.Int32);
        parameters.Add(nameof(relation.BuildActions), JsonSerializer.SerializeList(relation.BuildActions));
        parameters.Add(nameof(relation.ValidateActions), JsonSerializer.SerializeList(relation.ValidateActions));
        parameters.Add(nameof(relation.Attributes), JsonSerializer.SerializeNamedDictionary(relation.Attributes));
        parameters.Add(nameof(relation.Clusters), JsonSerializer.SerializeList(relation.Clusters));
        base.GetObjectData(relation, parameters);
    }
}