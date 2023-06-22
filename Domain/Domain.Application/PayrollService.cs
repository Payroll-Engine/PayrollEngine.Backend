using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public class PayrollService : ChildApplicationService<IPayrollRepository, Payroll>, IPayrollService
{
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; }
    public IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; }

    public PayrollService(
        IPayrollRepository payrollRepository,
        IGlobalCaseValueRepository globalCaseValueRepository,
        INationalCaseValueRepository nationalCaseValueRepository,
        ICompanyCaseValueRepository companyCaseValueRepository,
        IEmployeeCaseValueRepository employeeCaseValueRepository) :
        base(payrollRepository)
    {
        GlobalCaseValueRepository = globalCaseValueRepository ?? throw new ArgumentNullException(nameof(globalCaseValueRepository));
        NationalCaseValueRepository = nationalCaseValueRepository ?? throw new ArgumentNullException(nameof(nationalCaseValueRepository));
        CompanyCaseValueRepository = companyCaseValueRepository ?? throw new ArgumentNullException(nameof(companyCaseValueRepository));
        EmployeeCaseValueRepository = employeeCaseValueRepository ?? throw new ArgumentNullException(nameof(employeeCaseValueRepository));
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(IDbContext context, PayrollQuery query) =>
        await Repository.GetDerivedRegulationsAsync(context, query);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Case>> GetDerivedCasesAsync(IDbContext context, PayrollQuery query, CaseType? caseType = null,
        IEnumerable<string> caseNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCasesAsync(context, query, caseType, caseNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseFieldsAsync(context, query, caseFieldNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsOfCaseAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseNames, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseFieldsOfCaseAsync(context, query, caseNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<CaseRelation>> GetDerivedCaseRelationsAsync(IDbContext context, PayrollQuery query,
        string sourceCaseName = null, string targetCaseName = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseRelationsAsync(context, query, sourceCaseName, targetCaseName, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<WageType>> GetDerivedWageTypesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<decimal> wageTypeNumbers = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedWageTypesAsync(context, query, wageTypeNumbers, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Collector>> GetDerivedCollectorsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> collectorNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCollectorsAsync(context, query, collectorNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Lookup>> GetDerivedLookupsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedLookupsAsync(context, query, lookupNames, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<LookupValue>> GetDerivedLookupValuesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, IEnumerable<string> lookupKeys = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedLookupValuesAsync(context, query, lookupNames, lookupKeys, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedReportsAsync(context, query, reportNames, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ReportParameter>> GetDerivedReportParametersAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedReportParametersAsync(context, query, reportNames, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ReportTemplate>> GetDerivedReportTemplateAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, Language? language = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedReportTemplatesAsync(context, query, reportNames, language, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Script>> GetDerivedScriptsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedScriptsAsync(context, query, scriptNames, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ActionInfo>> GetDerivedScriptActionsAsync(IDbContext context, PayrollQuery query,
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

            var scriptActions = ActionParser.Parse(derivedScript.Value, functionType);
            actions.AddRange(scriptActions);
        }

        return actions;
    }
}