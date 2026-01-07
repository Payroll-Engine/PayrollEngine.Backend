using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Scripting.Action;

namespace PayrollEngine.Domain.Application;

public class PayrollService(IPayrollRepository payrollRepository,
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        IEmployeeCaseValueRepository employeeCaseValueRepository)
    : ChildApplicationService<IPayrollRepository, Payroll>(payrollRepository), IPayrollService
{
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; } = globalCaseValueRepository ?? throw new ArgumentNullException(nameof(globalCaseValueRepository));
    public INationalCaseValueRepository NationalCaseValueRepository { get; } = nationalCaseValueRepository ?? throw new ArgumentNullException(nameof(nationalCaseValueRepository));
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; } = companyCaseValueRepository ?? throw new ArgumentNullException(nameof(companyCaseValueRepository));
    public IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; } = employeeCaseValueRepository ?? throw new ArgumentNullException(nameof(employeeCaseValueRepository));

    /// <inheritdoc />
    public async Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(IDbContext context, PayrollQuery query) =>
        await Repository.GetDerivedRegulationsAsync(context, query);

    /// <inheritdoc />
    public async Task<IEnumerable<Case>> GetDerivedCasesAsync(IDbContext context, PayrollQuery query, CaseType? caseType = null,
        IEnumerable<string> caseNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCasesAsync(context, query, caseType, caseNames, overrideType, clusterSet);

    /// <inheritdoc />
    public async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseFieldsAsync(context, query, caseFieldNames, overrideType, clusterSet);

    /// <inheritdoc />
    public async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsOfCaseAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseNames, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseFieldsOfCaseAsync(context, query, caseNames, overrideType, clusterSet);

    /// <inheritdoc />
    public async Task<IEnumerable<CaseRelation>> GetDerivedCaseRelationsAsync(IDbContext context, PayrollQuery query,
        string sourceCaseName = null, string targetCaseName = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseRelationsAsync(context, query, sourceCaseName, targetCaseName, overrideType, clusterSet);

    /// <inheritdoc />
    public async Task<IEnumerable<WageType>> GetDerivedWageTypesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<decimal> wageTypeNumbers = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedWageTypesAsync(context, query, wageTypeNumbers, overrideType, clusterSet);

    /// <inheritdoc />
    public async Task<IEnumerable<Collector>> GetDerivedCollectorsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> collectorNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCollectorsAsync(context, query, collectorNames, overrideType, clusterSet);

    /// <inheritdoc />
    public async Task<IEnumerable<Lookup>> GetDerivedLookupsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedLookupsAsync(context, query, lookupNames, overrideType);

    /// <inheritdoc />
    public async Task<IEnumerable<LookupValue>> GetDerivedLookupValuesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, IEnumerable<string> lookupKeys = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedLookupValuesAsync(context, query, lookupNames, lookupKeys, overrideType);

    /// <inheritdoc />
    public async Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null, UserType? userType = null,
        ClusterSet clusterSet = null) =>
        await Repository.GetDerivedReportsAsync(context, query, reportNames, overrideType, userType, clusterSet);

    /// <inheritdoc />
    public async Task<IEnumerable<ReportParameter>> GetDerivedReportParametersAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedReportParametersAsync(context, query, reportNames, overrideType);

    /// <inheritdoc />
    public async Task<IEnumerable<ReportTemplate>> GetDerivedReportTemplateAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, string culture = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedReportTemplatesAsync(context, query, reportNames, culture, overrideType);

    /// <inheritdoc />
    public async Task<IEnumerable<Script>> GetDerivedScriptsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedScriptsAsync(context, query, scriptNames, overrideType);

    /// <inheritdoc />
    public async Task<IEnumerable<ActionInfo>> GetDerivedScriptActionsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null, FunctionType functionType = FunctionType.All)
    {
        var scripts = await GetDerivedScriptsAsync(context, query, scriptNames, overrideType);

        var actions = new List<ActionInfo>();
        foreach (var derivedScript in scripts)
        {
            if (string.IsNullOrWhiteSpace(derivedScript.Value))
            {
                continue;
            }

            var scriptActions = ActionReflector.GetActionInfo(derivedScript.Value, functionType);
            actions.AddRange(scriptActions);
        }

        return actions;
    }
}