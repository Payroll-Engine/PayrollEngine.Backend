using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CaseFieldRepository(IRegulationRepository regulationRepository, ICaseRepository caseRepository,
    ICaseFieldAuditRepository auditRepository, bool auditDisabled) :
    TrackChildDomainRepository<CaseField, CaseFieldAudit>(regulationRepository, DbSchema.Tables.CaseField,
        DbSchema.CaseFieldColumn.CaseId, auditRepository, auditDisabled), ICaseFieldRepository
{
    private ICaseRepository CaseRepository { get; } = caseRepository;

    public async Task<bool> ExistsAnyAsync(IDbContext context, int caseId, IEnumerable<string> caseFieldNames) =>
        await ExistsAnyAsync(context, DbSchema.CaseFieldColumn.CaseId, caseId, DbSchema.CaseFieldColumn.Name, caseFieldNames);

    public async Task<IEnumerable<CaseField>> GetRegulationCaseFieldsAsync(IDbContext context, int tenantId,
        IEnumerable<string> caseFieldNames, int? regulationId = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }
        if (caseFieldNames == null)
        {
            throw new ArgumentNullException(nameof(caseFieldNames));
        }

        var query = DbQueryFactory.NewQuery(TableName)
            .Select(GetColumnName("*"))
            .Join(DbSchema.Tables.Case,
                GetColumnName(DbSchema.CaseFieldColumn.CaseId),
                GetIdColumnName(DbSchema.Tables.Case))
            .Join(DbSchema.Tables.Regulation,
                GetColumnName(DbSchema.Tables.Case, DbSchema.CaseColumn.RegulationId),
                GetIdColumnName(DbSchema.Tables.Regulation))
            .Where(DbSchema.RegulationColumn.TenantId, tenantId)
            .WhereIn(GetColumnName(DbSchema.CaseFieldColumn.Name), caseFieldNames);

        if (regulationId.HasValue)
        {
            query = query.Where(GetColumnName(DbSchema.Tables.Regulation, DbSchema.ObjectColumn.Id), regulationId.Value);
        }

        var compileQuery = CompileQuery(query);
        var cases = await QueryAsync<CaseField>(context, compileQuery);
        return cases;
    }

    protected override void GetObjectCreateData(CaseField caseField, DbParameterCollection parameters)
    {
        parameters.Add(nameof(caseField.Name), caseField.Name);
        base.GetObjectCreateData(caseField, parameters);
    }

    protected override void GetObjectData(CaseField caseField, DbParameterCollection parameters)
    {
        // local fields
        // keep in sync with object properties
        parameters.Add(nameof(caseField.NameLocalizations), JsonSerializer.SerializeNamedDictionary(caseField.NameLocalizations));
        parameters.Add(nameof(caseField.Description), caseField.Description);
        parameters.Add(nameof(caseField.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(caseField.DescriptionLocalizations));
        parameters.Add(nameof(caseField.ValueType), caseField.ValueType, DbType.Int32);
        parameters.Add(nameof(caseField.ValueScope), caseField.ValueScope, DbType.Int32);
        parameters.Add(nameof(caseField.TimeType), caseField.TimeType, DbType.Int32);
        parameters.Add(nameof(caseField.TimeUnit), caseField.TimeUnit, DbType.Int32);
        parameters.Add(nameof(caseField.PeriodAggregation), caseField.PeriodAggregation, DbType.Int32);
        parameters.Add(nameof(caseField.OverrideType), caseField.OverrideType, DbType.Int32);
        parameters.Add(nameof(caseField.CancellationMode), caseField.CancellationMode, DbType.Int32);
        parameters.Add(nameof(caseField.ValueCreationMode), caseField.ValueCreationMode, DbType.Int32);
        parameters.Add(nameof(caseField.Culture), caseField.Culture);
        parameters.Add(nameof(caseField.ValueMandatory), caseField.ValueMandatory, DbType.Boolean);
        parameters.Add(nameof(caseField.Order), caseField.Order, DbType.Int32);
        parameters.Add(nameof(caseField.StartDateType), caseField.StartDateType, DbType.Int32);
        parameters.Add(nameof(caseField.EndDateType), caseField.EndDateType, DbType.Int32);
        parameters.Add(nameof(caseField.EndMandatory), caseField.EndMandatory, DbType.Boolean);
        parameters.Add(nameof(caseField.DefaultStart), caseField.DefaultStart);
        parameters.Add(nameof(caseField.DefaultEnd), caseField.DefaultEnd);
        parameters.Add(nameof(caseField.DefaultValue), caseField.DefaultValue);
        parameters.Add(nameof(caseField.LookupSettings), caseField.LookupSettings);
        // json collections
        parameters.Add(nameof(caseField.Tags), JsonSerializer.SerializeList(caseField.Tags));
        parameters.Add(nameof(caseField.Clusters), JsonSerializer.SerializeList(caseField.Clusters));
        parameters.Add(nameof(caseField.Attributes), JsonSerializer.SerializeNamedDictionary(caseField.Attributes));
        parameters.Add(nameof(caseField.ValueAttributes), JsonSerializer.SerializeNamedDictionary(caseField.ValueAttributes));

        // base fields
        base.GetObjectData(caseField, parameters);
    }

    public override async Task<CaseField> CreateAsync(IDbContext context, int caseId, CaseField caseField)
    {
        await EnsureNamespaceAsync(context, caseId, caseField);
        return await base.CreateAsync(context, caseId, caseField);
    }

    public override async Task<CaseField> UpdateAsync(IDbContext context, int caseId, CaseField caseField)
    {
        await EnsureNamespaceAsync(context, caseId, caseField);
        return await base.UpdateAsync(context, caseId, caseField);
    }

    private async Task EnsureNamespaceAsync(IDbContext context, int caseId, CaseField caseField)
    {
        // regulation
        var regulationId = await CaseRepository.GetParentIdAsync(context, caseId);
        if (!regulationId.HasValue)
        {
            return;
        }

        // namespace
        await ApplyNamespaceAsync(context, regulationId.Value, caseField);
    }
}