using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CaseRepository(IRegulationRepository regulationRepository,
    IScriptRepository scriptRepository, ICaseAuditRepository auditRepository, bool auditDisabled)
    : ScriptTrackChildDomainRepository<Case, CaseAudit>(DbSchema.Tables.Case, DbSchema.CaseColumn.RegulationId,
        regulationRepository, scriptRepository, auditRepository, auditDisabled), ICaseRepository
{
    public async Task<IEnumerable<Case>> QueryAsync(IDbContext context, int tenantId, string caseName, int? regulationId = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }

        var query = DbQueryFactory.NewQuery(TableName)
            .Select(GetColumnName("*"))
            .Join(DbSchema.Tables.Regulation,
                GetColumnName(DbSchema.CaseColumn.RegulationId),
                GetIdColumnName(DbSchema.Tables.Regulation))
            .Where(DbSchema.RegulationColumn.TenantId, tenantId)
            .Where(GetColumnName(DbSchema.CaseColumn.Name), caseName);

        if (regulationId.HasValue)
        {
            query = query.Where(GetColumnName(DbSchema.Tables.Regulation, DbSchema.ObjectColumn.Id), regulationId.Value);
        }

        var compileQuery = CompileQuery(query);
        var cases = await QueryAsync<Case>(context, compileQuery);
        return cases;
    }

    protected override void GetObjectCreateData(Case @case, DbParameterCollection parameters)
    {
        parameters.Add(nameof(@case.CaseType), @case.CaseType, DbType.Int32);
        parameters.Add(nameof(@case.Name), @case.Name);
        base.GetObjectCreateData(@case, parameters);
    }

    protected override void GetObjectData(Case @case, DbParameterCollection parameters)
    {
        // local fields
        // keep in sync with object properties
        parameters.Add(nameof(@case.NameLocalizations), JsonSerializer.SerializeNamedDictionary(@case.NameLocalizations));
        parameters.Add(nameof(@case.NameSynonyms), JsonSerializer.SerializeList(@case.NameSynonyms));
        parameters.Add(nameof(@case.Description), @case.Description);
        parameters.Add(nameof(@case.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(@case.DescriptionLocalizations));
        parameters.Add(nameof(@case.DefaultReason), @case.DefaultReason);
        parameters.Add(nameof(@case.DefaultReasonLocalizations), JsonSerializer.SerializeNamedDictionary(@case.DefaultReasonLocalizations));
        parameters.Add(nameof(@case.BaseCase), @case.BaseCase);
        parameters.Add(nameof(@case.BaseCaseFields), JsonSerializer.SerializeList(@case.BaseCaseFields));
        parameters.Add(nameof(@case.OverrideType), @case.OverrideType, DbType.Int32);
        parameters.Add(nameof(@case.CancellationType), @case.CancellationType, DbType.Int32);
        parameters.Add(nameof(@case.Hidden), @case.Hidden, DbType.Boolean);
        parameters.Add(nameof(@case.AvailableExpression), @case.AvailableExpression);
        parameters.Add(nameof(@case.BuildExpression), @case.BuildExpression);
        parameters.Add(nameof(@case.ValidateExpression), @case.ValidateExpression);
        parameters.Add(nameof(@case.Lookups), JsonSerializer.SerializeList(@case.Lookups));
        parameters.Add(nameof(@case.Slots), DefaultJsonSerializer.Serialize(@case.Slots));
        parameters.Add(nameof(@case.Script), @case.Script);
        parameters.Add(nameof(@case.ScriptVersion), @case.ScriptVersion);
        parameters.Add(nameof(@case.Binary), @case.Binary, DbType.Binary);
        parameters.Add(nameof(@case.ScriptHash), @case.ScriptHash, DbType.Int32);
        parameters.Add(nameof(@case.AvailableActions), JsonSerializer.SerializeList(@case.AvailableActions));
        parameters.Add(nameof(@case.BuildActions), JsonSerializer.SerializeList(@case.BuildActions));
        parameters.Add(nameof(@case.ValidateActions), JsonSerializer.SerializeList(@case.ValidateActions));
        parameters.Add(nameof(@case.Attributes), JsonSerializer.SerializeNamedDictionary(@case.Attributes));
        parameters.Add(nameof(@case.Clusters), JsonSerializer.SerializeList(@case.Clusters));

        // base fields
        base.GetObjectData(@case, parameters);
    }
}