using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides a case field
/// </summary>
public interface ICaseFieldProvider
{
    /// <summary>
    /// Get case type of case field
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case type</returns>
    Task<CaseType?> GetCaseTypeAsync(IDbContext context, string caseFieldName);

    /// <summary>
    /// Determine the case field
    /// If no case filed has a value expression, it returns the nm ost derived case field
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    Task<CaseField> GetCaseFieldAsync(IDbContext context, string caseFieldName);

    /// <summary>
    /// Determine the case field containing a value expression.
    /// If no case filed has a value expression, it returns the nm ost derived case field
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    Task<CaseField> GetValueCaseFieldAsync(IDbContext context, string caseFieldName);

    /// <summary>
    /// The available case fields by case type
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="caseType">The case type</param>
    Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, CaseType caseType);
}

