using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ScriptAuditRepository() : AuditChildDomainRepository<ScriptAudit>(DbSchema.Tables.ScriptAudit,
    DbSchema.ScriptAuditColumn.ScriptId), IScriptAuditRepository
{
    protected override void GetObjectCreateData(ScriptAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.ScriptId), audit.ScriptId, DbType.Int32);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.FunctionTypeMask), audit.FunctionTypes.ToBitmask(), DbType.Int64);
        parameters.Add(nameof(audit.Value), audit.Value);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType, DbType.Int32);
        base.GetObjectCreateData(audit, parameters);
    }
}