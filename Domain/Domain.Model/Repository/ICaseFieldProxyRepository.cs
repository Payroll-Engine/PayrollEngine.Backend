using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Cache for case fields
/// </summary>
public interface ICaseFieldProxyRepository
{
    /// <summary>
    /// The evaluation date
    /// </summary>
    DateTime EvaluationDate { get; }

    /// <summary>
    /// Get case type of a case field
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case type</returns>
    Task<CaseType?> GetCaseTypeAsync(string caseFieldName);

    /// <summary>
    /// Get id of the parent case
    /// </summary>
    /// <param name="caseFieldId">The case field object id</param>
    /// <returns>The id of the parent case</returns>
    Task<int?> GetParentCaseIdAsync(int caseFieldId);

    /// <summary>
    /// The available case fields by case type
    /// </summary>
    /// <param name="caseType">The case type</param>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(CaseType caseType);

    /// <summary>
    /// Get a case field by his name, considering derived regulations
    /// </summary>
    /// <param name="caseFieldName">The case field name (default: all)</param>
    /// <returns>The matching case field, null if no field was found</returns>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(string caseFieldName);
}