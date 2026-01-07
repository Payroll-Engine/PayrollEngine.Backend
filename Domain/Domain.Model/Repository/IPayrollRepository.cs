using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payrolls
/// </summary>
public interface IPayrollRepository : IChildDomainRepository<Payroll>
{
    /// <summary>
    /// Get id of the parent case
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseFieldId">The case field object id</param>
    /// <returns>The id of the parent case</returns>
    Task<int?> GetParentCaseIdAsync(IDbContext context, int caseFieldId);

    /// <summary>
    /// Get all active derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <returns>The derived cases</returns>
    Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(IDbContext context, PayrollQuery query);

    /// <summary>
    /// Get all active derived cases, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="caseType">The case type (default: all)</param>
    /// <param name="caseNames">The case names (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <param name="hidden">Hidden cases (default: all)</param>
    /// <returns>The derived cases</returns>
    Task<IEnumerable<DerivedCase>> GetDerivedCasesAsync(IDbContext context, PayrollQuery query,
        CaseType? caseType = null, IEnumerable<string> caseNames = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null, bool? hidden = null);

    /// <summary>
    /// Get a case field by his name, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="caseFieldNames">The case field names (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The matching case field, null if no field was found</returns>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get a case field by his name, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="caseNames">The case names</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The matching case field, null if no field was found</returns>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsOfCaseAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> caseNames, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get all active case relations, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="sourceCaseName">The relation source case name (default: all)</param>
    /// <param name="targetCaseName">The relation target case name (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived case relations</returns>
    Task<IEnumerable<DerivedCaseRelation>> GetDerivedCaseRelationsAsync(IDbContext context, PayrollQuery query,
        string sourceCaseName = null,
        string targetCaseName = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get from all wage types the topmost derived regulation
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="wageTypeNumbers">The wage type numbers (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived wage types</returns>
    Task<IEnumerable<DerivedWageType>> GetDerivedWageTypesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<decimal> wageTypeNumbers = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get all active collectors, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="collectorNames">The collector names filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived collectors</returns>
    Task<IEnumerable<DerivedCollector>> GetDerivedCollectorsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> collectorNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get all active lookups, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="lookupNames">The lookup names filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <returns>The derived lookups</returns>
    Task<IEnumerable<DerivedLookup>> GetDerivedLookupsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null);

    /// <summary>
    /// Get all active lookup values, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="lookupNames">The lookup names filter (default: all)</param>
    /// <param name="lookupKeys">The lookup-value keys filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <returns>The derived lookup values</returns>
    Task<IEnumerable<DerivedLookupValue>> GetDerivedLookupValuesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> lookupNames = null, IEnumerable<string> lookupKeys = null, OverrideType? overrideType = null);

    /// <summary>
    /// Get all active reports, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="reportNames">The report names filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="userType">The user type (default: all)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The derived reports</returns>
    Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null, 
        UserType? userType = null, ClusterSet clusterSet = null);

    /// <summary>
    /// Get all active report parameters, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="reportNames">The report names filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <returns>The derived report parameters</returns>
    Task<IEnumerable<DerivedReportParameter>> GetDerivedReportParametersAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null);

    /// <summary>
    /// Get all active report templates, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="reportNames">The report names filter (default: all)</param>
    /// <param name="culture">The report culture (default: current)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <returns>The derived report templates</returns>
    Task<IEnumerable<DerivedReportTemplate>> GetDerivedReportTemplatesAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> reportNames = null, string culture = null, OverrideType? overrideType = null);

    /// <summary>
    /// Get all active scripts, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The payroll query</param>
    /// <param name="scriptNames">The script names filter (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <returns>The derived scripts</returns>
    Task<IEnumerable<DerivedScript>> GetDerivedScriptsAsync(IDbContext context, PayrollQuery query,
        IEnumerable<string> scriptNames = null, OverrideType? overrideType = null);
}