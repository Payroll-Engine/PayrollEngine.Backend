using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class PayrollRepository : ChildDomainRepository<Payroll>, IPayrollRepository
{
    public IPayrollLayerRepository PayrollLayerRepository { get; }
    public IRegulationRepository RegulationRepository { get; }
    public ICaseFieldRepository CaseFieldRepository { get; }
    public IReportSetRepository ReportRepository { get; }
    public IScriptRepository ScriptRepository { get; }

    public PayrollRepository(IPayrollLayerRepository payrollLayerRepository, IRegulationRepository regulationRepository,
        ICaseFieldRepository caseFieldRepository, IReportSetRepository reportRepository, IScriptRepository scriptRepository,
        IDbContext context) :
        base(DbSchema.Tables.Payroll, DbSchema.PayrollColumn.TenantId, context)
    {
        PayrollLayerRepository = payrollLayerRepository ?? throw new ArgumentNullException(nameof(payrollLayerRepository));
        RegulationRepository = regulationRepository ?? throw new ArgumentNullException(nameof(regulationRepository));
        CaseFieldRepository = caseFieldRepository ?? throw new ArgumentNullException(nameof(caseFieldRepository));
        ReportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        ScriptRepository = scriptRepository ?? throw new ArgumentNullException(nameof(scriptRepository));
    }

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
        parameters.Add(nameof(payroll.CalendarCalculationMode), payroll.CalendarCalculationMode);
        parameters.Add(nameof(payroll.Country), payroll.Country);
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
    public async Task<int?> GetParentCaseIdAsync(int caseFieldId) =>
        await CaseFieldRepository.GetParentIdAsync(caseFieldId);

    #region Derived

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(PayrollQuery query) =>
        await new PayrollRepositoryRegulationCommand(Connection).GetDerivedRegulationsAsync(
            RegulationRepository, PayrollLayerRepository, query);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<Case>> GetDerivedCasesAsync(PayrollQuery query, CaseType? caseType = null,
        IEnumerable<string> caseNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryCaseCommand(Connection).GetDerivedCasesAsync(query, caseType,
            caseNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryCaseFieldCommand(Connection).GetDerivedCaseFieldsAsync(query,
            caseFieldNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsOfCaseAsync(PayrollQuery query,
        IEnumerable<string> caseNames, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryFieldOfCaseCommand(Connection).GetDerivedFieldsOfCaseAsync(query,
            caseNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<CaseRelation>> GetDerivedCaseRelationsAsync(PayrollQuery query,
        string sourceCaseName = null, string targetCaseName = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryCaseRelationCommand(Connection).GetDerivedCaseRelationsAsync(query,
            sourceCaseName, targetCaseName, overrideType, clusterSet);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<WageType>> GetDerivedWageTypesAsync(PayrollQuery query,
        IEnumerable<decimal> wageTypeNumbers = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryWageTypeCommand(Connection).GetDerivedWageTypesAsync(query,
            wageTypeNumbers, overrideType, clusterSet);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<Collector>> GetDerivedCollectorsAsync(PayrollQuery query,
        IEnumerable<string> collectorNames = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryCollectorCommand(Connection).GetDerivedCollectorsAsync(query,
            collectorNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<Lookup>> GetDerivedLookupsAsync(PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null) =>
        await new PayrollRepositoryLookupCommand(Connection).GetDerivedLookupsAsync(query,
            lookupNames, overrideType);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(PayrollQuery query,
        IEnumerable<string> reportNames = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await new PayrollRepositoryReportCommand(Connection).GetDerivedReportsAsync(ReportRepository,
            query, reportNames, overrideType, clusterSet);

    /// <inheritdoc/>
    public virtual async Task<ReportTemplate> GetDerivedReportTemplateAsync(PayrollQuery query,
        Language language, IEnumerable<string> reportNames = null) =>
        await new PayrollRepositoryReportTemplateCommand(Connection).GetDerivedReportTemplateAsync(
            query, language, reportNames);

    public virtual async Task<IEnumerable<Script>> GetDerivedScriptsAsync(PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null) =>
        await new PayrollRepositoryScriptCommand(Connection).GetDerivedScriptsAsync(ScriptRepository,
            query, scriptNames, overrideType);

    #endregion
}