using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides a case field
/// </summary>
public sealed class CaseFieldProvider
{
    /// <summary>
    /// The case field repository
    /// </summary>
    public ICaseFieldProxyRepository CaseFieldRepository { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseFieldRepository"></param>
    public CaseFieldProvider(ICaseFieldProxyRepository caseFieldRepository)
    {
        CaseFieldRepository = caseFieldRepository ?? throw new ArgumentNullException(nameof(caseFieldRepository));
    }

    /// <summary>
    /// Get case type of a case field
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case type</returns>
    public async Task<CaseType?> GetCaseTypeAsync(string caseFieldName) =>
        await CaseFieldRepository.GetCaseTypeAsync(caseFieldName);

    /// <summary>
    /// Get id of the parent case
    /// </summary>
    /// <param name="caseFieldId">The case field object id</param>
    /// <returns>The id of the parent case</returns>
    public async Task<int?> GetParentCaseIdAsync(int caseFieldId) =>
        await CaseFieldRepository.GetParentCaseIdAsync(caseFieldId);

    /// <summary>
    /// Determine the case field
    /// If no case filed has a value expression, it returns the nm ost derived case field
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    public async Task<CaseField> GetCaseFieldAsync(string caseFieldName)
    {
        // derived case fields (ignore case fields created after the evaluation date)
        var derivedCaseFields = (await CaseFieldRepository.GetDerivedCaseFieldsAsync(caseFieldName)).ToList();
        if (derivedCaseFields.Count == 0)
        {
            Log.Debug($"Missing case field with name {caseFieldName}");
            return null;
        }
        return derivedCaseFields.First();
    }

    /// <summary>
    /// Determine the case field containing a value expression.
    /// If no case filed has a value expression, it returns the nm ost derived case field
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    public async Task<CaseField> GetValueCaseFieldAsync(string caseFieldName)
    {
        // derived case fields (ignore case fields created after the evaluation date)
        var derivedCaseFields = (await CaseFieldRepository.GetDerivedCaseFieldsAsync(caseFieldName)).ToList();
        if (derivedCaseFields.Count == 0)
        {
            Log.Debug($"Missing case field with name {caseFieldName}");
            return null;
        }

        // no derived case field available
        if (derivedCaseFields.Count == 1)
        {
            return derivedCaseFields.First();
        }

        // search within derived case field for the record with the value expression
        var caseFieldLookup = derivedCaseFields.ToLookup(cf => cf.Name, cf => cf).FirstOrDefault();
        return caseFieldLookup.GetNewestObject(CaseFieldRepository.EvaluationDate);
    }

}