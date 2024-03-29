﻿using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ScriptAuditRepository() : AuditChildDomainRepository<ScriptAudit>(DbSchema.Tables.ScriptAudit,
    DbSchema.ScriptAuditColumn.ScriptId), IScriptAuditRepository
{
    protected override void GetObjectCreateData(ScriptAudit audit, DbParameterCollection parameters)
    {
        parameters.Add(nameof(audit.ScriptId), audit.ScriptId);
        parameters.Add(nameof(audit.Name), audit.Name);
        parameters.Add(nameof(audit.FunctionTypeMask), audit.FunctionTypes.ToBitmask());
        parameters.Add(nameof(audit.Value), audit.Value);
        parameters.Add(nameof(audit.OverrideType), audit.OverrideType);
        base.GetObjectCreateData(audit, parameters);
    }
}