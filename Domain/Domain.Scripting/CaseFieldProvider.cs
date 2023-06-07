using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides a case field
/// </summary>
public sealed class CaseFieldProvider : ICaseFieldProvider
{
    /// <summary>
    /// The case field repository
    /// </summary>
    private ICaseFieldProxyRepository CaseFieldRepository { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="caseFieldRepository"></param>
    public CaseFieldProvider(ICaseFieldProxyRepository caseFieldRepository)
    {
        CaseFieldRepository = caseFieldRepository ?? throw new ArgumentNullException(nameof(caseFieldRepository));
    }

    /// <inheritdoc />
    public async Task<CaseType?> GetCaseTypeAsync(IDbContext context, string caseFieldName) =>
        await CaseFieldRepository.GetCaseTypeAsync(context, caseFieldName);

    /// <inheritdoc />
    public async Task<CaseField> GetCaseFieldAsync(IDbContext context, string caseFieldName)
    {
        // derived case fields (ignore case fields created after the evaluation date)
        var derivedCaseFields = (await CaseFieldRepository.GetDerivedCaseFieldsAsync(context, caseFieldName)).ToList();
        if (derivedCaseFields.Count == 0)
        {
            Log.Debug($"Missing case field with name {caseFieldName}");
            return null;
        }
        return derivedCaseFields.First();
    }

    /// <inheritdoc />
    public async Task<CaseField> GetValueCaseFieldAsync(IDbContext context, string caseFieldName)
    {
        // derived case fields (ignore case fields created after the evaluation date)
        var derivedCaseFields = (await CaseFieldRepository.GetDerivedCaseFieldsAsync(context, caseFieldName)).ToList();
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

    /// <inheritdoc />
    public async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, CaseType caseType) =>
        await CaseFieldRepository.GetDerivedCaseFieldsAsync(context, caseType);
}