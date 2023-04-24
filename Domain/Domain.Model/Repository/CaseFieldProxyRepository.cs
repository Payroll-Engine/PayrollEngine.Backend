//#define CASE_FIELD_LOAD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Cache for case fields
/// </summary>
public class CaseFieldProxyRepository : ICaseFieldProxyRepository
{
    private sealed class CaseFieldKey : Tuple<int, string>
    {
        internal CaseFieldKey(int payrollId, string caseFieldName) :
            // remove slot from the case field name
            base(payrollId, new CaseValueReference(caseFieldName).CaseFieldName)
        {
        }
    }

    private IDictionary<CaseFieldKey, List<ChildCaseField>> derivedCaseFields;

    /// <summary>
    /// The payroll repository
    /// </summary>
    public IPayrollRepository PayrollRepository { get; }

    /// <summary>
    /// The tenant id
    /// </summary>
    public int TenantId { get; }

    /// <summary>
    /// The payroll id
    /// </summary>
    public int PayrollId { get; }

    /// <summary>
    /// The regulation validation date
    /// </summary>
    public DateTime RegulationDate { get; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    public DateTime EvaluationDate { get; }

    /// <summary>
    /// The cluster set
    /// </summary>
    public ClusterSet ClusterSet { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseFieldProxyRepository"/> class
    /// </summary>
    /// <param name="payrollRepository">The payroll repository</param>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="payrollId">The payroll identifier</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date</param>
    /// <param name="clusterSet">The cluster set</param>
    public CaseFieldProxyRepository(IPayrollRepository payrollRepository, int tenantId, int payrollId,
        DateTime regulationDate, DateTime evaluationDate, ClusterSet clusterSet = null)
    {
        PayrollRepository = payrollRepository ?? throw new ArgumentNullException(nameof(payrollRepository));
        TenantId = tenantId;
        PayrollId = payrollId;
        RegulationDate = regulationDate.IsUtc() ? regulationDate : throw new ArgumentException(nameof(regulationDate));
        EvaluationDate = evaluationDate.IsUtc() ? evaluationDate : throw new ArgumentException(nameof(evaluationDate));
        ClusterSet = clusterSet;
    }

    /// <inheritdoc />
    public async Task<CaseType?> GetCaseTypeAsync(IDbContext context, string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        await EnsureCaseFieldsAsync(context);

        // case field
        var key = new CaseFieldKey(PayrollId, caseFieldName);
        if (derivedCaseFields.TryGetValue(key, out var field))
        {
            var caseField = field.FirstOrDefault();
            return caseField?.CaseType;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<int?> GetParentCaseIdAsync(IDbContext context, int caseFieldId) =>
        await PayrollRepository.GetParentCaseIdAsync(context, caseFieldId);

    /// <inheritdoc />
    public async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, CaseType caseType)
    {
        await EnsureCaseFieldsAsync(context);
        var caseFields = new List<ChildCaseField>();
        foreach (var value in derivedCaseFields.Values)
        {
            var caseFieldValue = value.FirstOrDefault();
            if (caseFieldValue == null || caseFieldValue.CaseType != caseType)
            {
                continue;
            }
            caseFields.AddRange(value);
        }
        return caseFields;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(IDbContext context, string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        await EnsureCaseFieldsAsync(context);

        // case field key
        var key = new CaseFieldKey(PayrollId, caseFieldName);
        if (!derivedCaseFields.ContainsKey(key))
        {
            return new List<ChildCaseField>();
        }
        return derivedCaseFields[key];
    }

    private async System.Threading.Tasks.Task EnsureCaseFieldsAsync(IDbContext context)
    {
        if (derivedCaseFields != null)
        {
            return;
        }

#if CASE_FIELD_LOAD
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif

        derivedCaseFields = new Dictionary<CaseFieldKey, List<ChildCaseField>>();

        // load derived case fields
        var caseFields = (await PayrollRepository.GetDerivedCaseFieldsAsync(context,
            new()
            {
                TenantId = TenantId,
                PayrollId = PayrollId,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            clusterSet: ClusterSet,
            overrideType: OverrideType.Active)).ToList();

        // group by case field name
        var caseFieldsByName = caseFields.GroupBy(x => x.Name, x => x);
        foreach (var caseFieldByName in caseFieldsByName)
        {
            derivedCaseFields.Add(new(PayrollId, caseFieldByName.Key), caseFieldByName.ToList());
        }

#if CASE_FIELD_LOAD
            stopwatch.Stop();
            PayrollEngine.Log.Information($"Load case fields ({caseFields.Count()} fields): {stopwatch.ElapsedMilliseconds} ms");
#endif
    }
}