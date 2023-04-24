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
    /// <param name="context">The database context</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case type</returns>
    Task<CaseType?> GetCaseTypeAsync(IDbContext context, string caseFieldName);

    /// <summary>
    /// Get id of the parent case
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseFieldId">The case field object id</param>
    /// <returns>The id of the parent case</returns>
    Task<int?> GetParentCaseIdAsync(IDbContext context, int caseFieldId);

    /// <summary>
    /// The available case fields by case type
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseType">The case type</param>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, CaseType caseType);

    /// <summary>
    /// Get a case field by his name, considering derived regulations
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseFieldName">The case field name (default: all)</param>
    /// <returns>The matching case field, null if no field was found</returns>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, string caseFieldName);
}