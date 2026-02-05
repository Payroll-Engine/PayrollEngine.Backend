using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class WageTypeRepository(IRegulationRepository regulationRepository,
    IScriptRepository scriptRepository, IWageTypeAuditRepository auditRepository, bool auditDisabled)
    : ScriptTrackChildDomainRepository<WageType, WageTypeAudit>(DbSchema.Tables.WageType,
        DbSchema.WageTypeColumn.RegulationId, regulationRepository, scriptRepository, auditRepository, auditDisabled), IWageTypeRepository
{
    protected override void GetObjectCreateData(WageType wageType, DbParameterCollection parameters)
    {
        parameters.Add(nameof(wageType.WageTypeNumber), wageType.WageTypeNumber);
        base.GetObjectCreateData(wageType, parameters);
    }

    protected override void GetObjectData(WageType wageType, DbParameterCollection parameters)
    {
        parameters.Add(nameof(wageType.Name), wageType.Name);
        parameters.Add(nameof(wageType.NameLocalizations), JsonSerializer.SerializeNamedDictionary(wageType.NameLocalizations));
        parameters.Add(nameof(wageType.Description), wageType.Description);
        parameters.Add(nameof(wageType.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(wageType.DescriptionLocalizations));
        parameters.Add(nameof(wageType.OverrideType), wageType.OverrideType, DbType.Int32);
        parameters.Add(nameof(wageType.ValueType), wageType.ValueType, DbType.Int32);
        parameters.Add(nameof(wageType.Calendar), wageType.Calendar);
        parameters.Add(nameof(wageType.Culture), wageType.Culture);
        parameters.Add(nameof(wageType.Collectors), JsonSerializer.SerializeList(wageType.Collectors));
        parameters.Add(nameof(wageType.CollectorGroups), JsonSerializer.SerializeList(wageType.CollectorGroups));
        parameters.Add(nameof(wageType.ValueExpression), wageType.ValueExpression);
        parameters.Add(nameof(wageType.ResultExpression), wageType.ResultExpression);
        parameters.Add(nameof(wageType.ValueActions), JsonSerializer.SerializeList(wageType.ValueActions));
        parameters.Add(nameof(wageType.ResultActions), JsonSerializer.SerializeList(wageType.ResultActions));
        parameters.Add(nameof(wageType.Script), wageType.Script);
        parameters.Add(nameof(wageType.ScriptVersion), wageType.ScriptVersion);
        parameters.Add(nameof(wageType.Binary), wageType.Binary, DbType.Binary);
        parameters.Add(nameof(wageType.ScriptHash), wageType.ScriptHash, DbType.Int32);
        parameters.Add(nameof(wageType.Attributes), JsonSerializer.SerializeNamedDictionary(wageType.Attributes));
        parameters.Add(nameof(wageType.Clusters), JsonSerializer.SerializeList(wageType.Clusters));
        base.GetObjectData(wageType, parameters);
    }

    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<decimal> wageTypeNumbers) =>
        await ExistsAnyAsync(context, DbSchema.WageTypeColumn.RegulationId, regulationId, DbSchema.WageTypeColumn.WageTypeNumber, wageTypeNumbers);
}