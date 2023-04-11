using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollService : IChildApplicationService<IPayrollRepository, Payroll>
{
    IGlobalCaseValueRepository GlobalCaseValueRepository { get; }
    INationalCaseValueRepository NationalCaseValueRepository { get; }
    ICompanyCaseValueRepository CompanyCaseValueRepository { get; }
    IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; }

    /// <summary>
    /// Get derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <returns>The derived lookups from the payroll</returns>
    Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(PayrollQuery query);

    /// <summary>
    /// Get all active derived cases, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="caseType">The case type (default: all)</param>
    /// <param name="caseNames">The case names (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived cases</returns>
    Task<IEnumerable<Case>> GetDerivedCasesAsync(PayrollQuery query,
        CaseType? caseType = null, IEnumerable<string> caseNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get a case field by his name, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="caseFieldNames">The case field names (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The matching case field, null if no field was found</returns>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get a case field by his name, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="caseNames">The case names</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The matching case field, null if no field was found</returns>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsOfCaseAsync(PayrollQuery query, IEnumerable<string> caseNames,
        OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get all active case relations, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="sourceCaseName">The relation source case name (default: all)</param>
    /// <param name="targetCaseName">The relation target case name (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived case relations</returns>
    Task<IEnumerable<CaseRelation>> GetDerivedCaseRelationsAsync(PayrollQuery query, string sourceCaseName = null,
        string targetCaseName = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get from all wage types the the topmost derived regulation
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="wageTypeNumbers">The wage type numbers (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived wage types</returns>
    Task<IEnumerable<WageType>> GetDerivedWageTypesAsync(PayrollQuery query,
        IEnumerable<decimal> wageTypeNumbers = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get all active collectors, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="collectorNames">The collector names filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived collectors</returns>
    Task<IEnumerable<Collector>> GetDerivedCollectorsAsync(PayrollQuery query,
        IEnumerable<string> collectorNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get derived lookups by lookup name
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="lookupNames">The lookup names filter (default: all)</param>
    /// <param name="overrideType">The override type (default: active)</param>
    /// <returns>The derived lookups from the payroll</returns>
    Task<IEnumerable<Lookup>> GetDerivedLookupsAsync(PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null);

    /// <summary>
    /// Get all active reports, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="reportNames">The report names filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived reports</returns>
    Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get all active reports, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="language">The report language</param>
    /// <param name="reportNames">The report names filter (default: all)</param>
    /// <returns>The derived reports</returns>
    Task<ReportTemplate> GetDerivedReportTemplateAsync(PayrollQuery query, Language language,
        IEnumerable<string> reportNames = null);

    /// <summary>
    /// Get all active scripts, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="scriptNames">The script names filter (default: all)</param>
    /// <param name="overrideType">The override type (default: active)</param>
    /// <returns>The derived scripts</returns>
    Task<IEnumerable<Script>> GetDerivedScriptsAsync(PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null);

    /// <summary>
    /// Get actions of active scripts, considering derived regulations
    /// </summary>
    /// <param name="query">The payroll query</param>
    /// <param name="scriptNames">The script names filter (default: all)</param>
    /// <param name="overrideType">The override type (default: active)</param>
    /// <param name="functionType">The function types (default: all)</param>
    /// <returns>The payroll action infos</returns>
    Task<IEnumerable<ActionInfo>> GetDerivedScriptActionsAsync(PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null, FunctionType functionType = FunctionType.All);
}