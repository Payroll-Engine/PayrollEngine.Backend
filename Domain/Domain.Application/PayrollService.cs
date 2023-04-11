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
    public virtual async Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(PayrollQuery query) =>
        await Repository.GetDerivedRegulationsAsync(query);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Case>> GetDerivedCasesAsync(PayrollQuery query, CaseType? caseType = null,
        IEnumerable<string> caseNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCasesAsync(query, caseType, caseNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseFieldsAsync(query, caseFieldNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsOfCaseAsync(PayrollQuery query,
        IEnumerable<string> caseNames, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseFieldsOfCaseAsync(query, caseNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<CaseRelation>> GetDerivedCaseRelationsAsync(PayrollQuery query, string sourceCaseName = null,
        string targetCaseName = null, OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCaseRelationsAsync(query, sourceCaseName, targetCaseName, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<WageType>> GetDerivedWageTypesAsync(PayrollQuery query, IEnumerable<decimal> wageTypeNumbers = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedWageTypesAsync(query, wageTypeNumbers, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Collector>> GetDerivedCollectorsAsync(PayrollQuery query, IEnumerable<string> collectorNames = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedCollectorsAsync(query, collectorNames, overrideType, clusterSet);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Lookup>> GetDerivedLookupsAsync(PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedLookupsAsync(query, lookupNames, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(PayrollQuery query, IEnumerable<string> reportNames = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await Repository.GetDerivedReportsAsync(query, reportNames, overrideType);

    /// <inheritdoc />
    public virtual async Task<ReportTemplate> GetDerivedReportTemplateAsync(PayrollQuery query, Language language,
        IEnumerable<string> reportNames = null) =>
        await Repository.GetDerivedReportTemplateAsync(query, language, reportNames);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Script>> GetDerivedScriptsAsync(PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null) =>
        await Repository.GetDerivedScriptsAsync(query, scriptNames, overrideType);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ActionInfo>> GetDerivedScriptActionsAsync(PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null, FunctionType functionType = FunctionType.All)
    {
        var scripts = await GetDerivedScriptsAsync(query, scriptNames, overrideType);

        var actions = new List<ActionInfo>();
        var actionParser = new ActionParser();
        foreach (var derivedScript in scripts)
        {
            if (string.IsNullOrWhiteSpace(derivedScript.Value))
            {
                continue;
            }

            var scriptActions = actionParser.Parse(derivedScript.Value, functionType);
            actions.AddRange(scriptActions);
        }

        return actions;
    }
}