using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Cache for derived payroll regulation objects (cases, fields, relations).
/// Used during bulk operations to avoid redundant database queries.
/// Regulation objects are immutable during case change processing.
/// </summary>
public sealed class DerivedPayrollCache
{
    // All derived cases for this payroll (all types, all names)
    private List<DerivedCase> AllCases { get; }

    // All derived case fields for this payroll
    private List<ChildCaseField> AllCaseFields { get; }

    // All derived case fields grouped by parent case
    private List<ChildCaseField> AllCaseFieldsOfCase { get; }

    // All derived case relations for this payroll
    private List<DerivedCaseRelation> AllCaseRelations { get; }

    private DerivedPayrollCache(
        List<DerivedCase> allCases,
        List<ChildCaseField> allCaseFields,
        List<ChildCaseField> allCaseFieldsOfCase,
        List<DerivedCaseRelation> allCaseRelations)
    {
        AllCases = allCases ?? throw new ArgumentNullException(nameof(allCases));
        AllCaseFields = allCaseFields ?? throw new ArgumentNullException(nameof(allCaseFields));
        AllCaseFieldsOfCase = allCaseFieldsOfCase ?? throw new ArgumentNullException(nameof(allCaseFieldsOfCase));
        AllCaseRelations = allCaseRelations ?? throw new ArgumentNullException(nameof(allCaseRelations));
    }

    /// <summary>
    /// Create and populate cache by loading all derived objects for a payroll
    /// </summary>
    public static async Task<DerivedPayrollCache> CreateAsync(
        IDbContext context,
        IPayrollRepository payrollRepository,
        PayrollQuery query,
        ClusterSet clusterSet = null)
    {
        // Load ALL derived cases (no name filter, no type filter)
        var allCases = (await payrollRepository.GetDerivedCasesAsync(
            context, query,
            caseType: null,
            caseNames: null,
            overrideType: OverrideType.Active,
            clusterSet: clusterSet)).ToList();

        // Load ALL derived case fields (no name filter)
        var allCaseFields = (await payrollRepository.GetDerivedCaseFieldsAsync(
            context, query,
            caseFieldNames: null,
            overrideType: OverrideType.Active,
            clusterSet: clusterSet)).ToList();

        // Load ALL derived case fields of case (all case names)
        var allCaseNames = allCases.Select(c => c.Name).Distinct();
        var allCaseFieldsOfCase = (await payrollRepository.GetDerivedCaseFieldsOfCaseAsync(
            context, query,
            caseNames: allCaseNames,
            overrideType: OverrideType.Active,
            clusterSet: clusterSet)).ToList();

        // Load ALL derived case relations (no source/target filter)
        var allCaseRelations = (await payrollRepository.GetDerivedCaseRelationsAsync(
            context, query,
            sourceCaseName: null,
            targetCaseName: null,
            overrideType: OverrideType.Active,
            clusterSet: clusterSet)).ToList();

        return new DerivedPayrollCache(allCases, allCaseFields, allCaseFieldsOfCase, allCaseRelations);
    }

    /// <summary>
    /// Get derived cases filtered by case type and/or names
    /// (replaces PayrollRepository.GetDerivedCasesAsync)
    /// </summary>
    public IEnumerable<DerivedCase> GetDerivedCases(
        CaseType? caseType = null,
        IEnumerable<string> caseNames = null)
    {
        IEnumerable<DerivedCase> result = AllCases;

        if (caseType.HasValue)
        {
            result = result.Where(c => c.CaseType == caseType.Value);
        }

        if (caseNames != null)
        {
            var nameSet = new HashSet<string>(caseNames);
            result = result.Where(c => nameSet.Contains(c.Name));
        }

        return result;
    }

    /// <summary>
    /// Get derived case fields filtered by field names
    /// (replaces PayrollRepository.GetDerivedCaseFieldsAsync)
    /// </summary>
    public IEnumerable<ChildCaseField> GetDerivedCaseFields(
        IEnumerable<string> caseFieldNames = null)
    {
        if (caseFieldNames == null)
        {
            return AllCaseFields;
        }

        var nameSet = new HashSet<string>(caseFieldNames);
        return AllCaseFields.Where(f => nameSet.Contains(f.Name));
    }

    /// <summary>
    /// Get derived case fields belonging to specific cases
    /// (replaces PayrollRepository.GetDerivedCaseFieldsOfCaseAsync)
    /// </summary>
    public IEnumerable<ChildCaseField> GetDerivedCaseFieldsOfCase(
        IEnumerable<string> caseNames)
    {
        if (caseNames == null)
        {
            return AllCaseFieldsOfCase;
        }

        var nameSet = new HashSet<string>(caseNames);
        var caseIds = new HashSet<int>(
            AllCases.Where(c => nameSet.Contains(c.Name)).Select(c => c.Id));
        return AllCaseFieldsOfCase.Where(f => caseIds.Contains(f.CaseId));
    }

    /// <summary>
    /// Get derived case relations filtered by source and/or target case name
    /// (replaces PayrollRepository.GetDerivedCaseRelationsAsync)
    /// </summary>
    public IEnumerable<DerivedCaseRelation> GetDerivedCaseRelations(
        string sourceCaseName = null,
        string targetCaseName = null)
    {
        IEnumerable<DerivedCaseRelation> result = AllCaseRelations;

        if (!string.IsNullOrWhiteSpace(sourceCaseName))
        {
            result = result.Where(r => string.Equals(r.SourceCaseName, sourceCaseName));
        }

        if (!string.IsNullOrWhiteSpace(targetCaseName))
        {
            result = result.Where(r => string.Equals(r.TargetCaseName, targetCaseName));
        }

        return result;
    }
}
