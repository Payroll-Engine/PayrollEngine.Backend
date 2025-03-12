using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class PayrollRepository(IPayrollLayerRepository payrollLayerRepository,
        IRegulationRepository regulationRepository,
        ICaseFieldRepository caseFieldRepository, IReportSetRepository reportRepository,
        IScriptRepository scriptRepository)
    : ChildDomainRepository<Payroll>(DbSchema.Tables.Payroll, DbSchema.PayrollColumn.TenantId), IPayrollRepository
{
    private IPayrollLayerRepository PayrollLayerRepository { get; } = payrollLayerRepository ?? throw new ArgumentNullException(nameof(payrollLayerRepository));
    private IRegulationRepository RegulationRepository { get; } = regulationRepository ?? throw new ArgumentNullException(nameof(regulationRepository));
    private ICaseFieldRepository CaseFieldRepository { get; } = caseFieldRepository ?? throw new ArgumentNullException(nameof(caseFieldRepository));
    private IReportSetRepository ReportRepository { get; } = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
    private IScriptRepository ScriptRepository { get; } = scriptRepository ?? throw new ArgumentNullException(nameof(scriptRepository));

    protected override void GetObjectCreateData(Payroll payroll, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payroll.DivisionId), payroll.DivisionId);
        base.GetObjectCreateData(payroll, parameters);
    }

    protected override void GetObjectData(Payroll payroll, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payroll.Name), payroll.Name);
        parameters.Add(nameof(payroll.NameLocalizations), JsonSerializer.SerializeNamedDictionary(payroll.NameLocalizations));
        parameters.Add(nameof(payroll.Description), payroll.Description);
        parameters.Add(nameof(payroll.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(payroll.DescriptionLocalizations));
        parameters.Add(nameof(payroll.ClusterSetCase), payroll.ClusterSetCase);
        parameters.Add(nameof(payroll.ClusterSetCaseField), payroll.ClusterSetCaseField);
        parameters.Add(nameof(payroll.ClusterSetCollector), payroll.ClusterSetCollector);
        parameters.Add(nameof(payroll.ClusterSetCollectorRetro), payroll.ClusterSetCollectorRetro);
        parameters.Add(nameof(payroll.ClusterSetWageType), payroll.ClusterSetWageType);
        parameters.Add(nameof(payroll.ClusterSetWageTypeRetro), payroll.ClusterSetWageTypeRetro);
        parameters.Add(nameof(payroll.ClusterSetCaseValue), payroll.ClusterSetCaseValue);
        parameters.Add(nameof(payroll.ClusterSetWageTypePeriod), payroll.ClusterSetWageTypePeriod);
        parameters.Add(nameof(payroll.ClusterSets), DefaultJsonSerializer.Serialize(payroll.ClusterSets));
        parameters.Add(nameof(payroll.Attributes), JsonSerializer.SerializeNamedDictionary(payroll.Attributes));
        base.GetObjectData(payroll, parameters);
    }

    /// <inheritdoc />
    public async Task<int?> GetParentCaseIdAsync(IDbContext context, int caseFieldId) =>
        await CaseFieldRepository.GetParentIdAsync(context, caseFieldId);

    #region Derived

    /// <inheritdoc/>
    public async Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(IDbContext context, PayrollQuery query) =>
        await new PayrollRepositoryRegulationCommand(context).GetDerivedRegulationsAsync(
            RegulationRepository, PayrollLayerRepository, query);

    /// <inheritdoc/>
    public async Task<IEnumerable<Case>> GetDerivedCasesAsync(IDbContext context, PayrollQuery query, CaseType? caseType = null,
        IEnumerable<string> caseNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null, bool? hidden = null) =>
        await new PayrollRepositoryCaseCommand(context).GetDerivedCasesAsync(query, caseType,
            caseNames, overrideType, clusterSet, hidden);

    /// <inheritdoc/>
    public async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryCaseFieldCommand(context).GetDerivedCaseFieldsAsync(query,
            caseFieldNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsOfCaseAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseNames, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryFieldOfCaseCommand(context).GetDerivedFieldsOfCaseAsync(query,
            caseNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public async Task<IEnumerable<CaseRelation>> GetDerivedCaseRelationsAsync(IDbContext context, PayrollQuery query,
        string sourceCaseName = null, string targetCaseName = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryCaseRelationCommand(context).GetDerivedCaseRelationsAsync(query,
            sourceCaseName, targetCaseName, overrideType, clusterSet);

    /// <inheritdoc/>
    public async Task<IEnumerable<WageType>> GetDerivedWageTypesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<decimal> wageTypeNumbers = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryWageTypeCommand(context).GetDerivedWageTypesAsync(query,
            wageTypeNumbers, overrideType, clusterSet);

    /// <inheritdoc/>
    public async Task<IEnumerable<Collector>> GetDerivedCollectorsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> collectorNames = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryCollectorCommand(context).GetDerivedCollectorsAsync(query,
            collectorNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public async Task<IEnumerable<Lookup>> GetDerivedLookupsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null) =>
        await new PayrollRepositoryLookupCommand(context).GetDerivedLookupsAsync(query,
            lookupNames, overrideType);

    /// <inheritdoc/>
    public async Task<IEnumerable<LookupValue>> GetDerivedLookupValuesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, IEnumerable<string> lookupKeys = null, OverrideType? overrideType = null) =>
        await new PayrollRepositoryLookupValuesCommand(context).GetDerivedLookupValuesAsync(query, lookupNames, lookupKeys, overrideType);

    /// <inheritdoc/>
    public async Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null,
        UserType? userType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryReportCommand(context).GetDerivedReportsAsync(ReportRepository,
            query, reportNames, overrideType, userType, clusterSet);

    /// <inheritdoc/>
    public async Task<IEnumerable<ReportParameter>> GetDerivedReportParametersAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null) =>
        await new PayrollRepositoryReportParametersCommand(context).GetDerivedReportParametersAsync(query, reportNames, overrideType);

    /// <inheritdoc/>
    public async Task<IEnumerable<ReportTemplate>> GetDerivedReportTemplatesAsync(IDbContext context, PayrollQuery query,
       IEnumerable<string> reportNames = null, string culture = null, OverrideType? overrideType = null) =>
        await new PayrollRepositoryReportTemplatesCommand(context).GetDerivedReportTemplatesAsync(
            query, reportNames, culture, overrideType);

    public async Task<IEnumerable<Script>> GetDerivedScriptsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null) =>
        await new PayrollRepositoryScriptCommand(context).GetDerivedScriptsAsync(ScriptRepository,
            query, scriptNames, overrideType);

    #endregion
}