using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides a case
/// </summary>
public sealed class CaseProvider : ICaseProvider
{
    /// <summary>
    /// The tenant
    /// </summary>
    private Tenant Tenant { get; }

    /// <summary>
    /// The regulation date
    /// </summary>
    private DateTime RegulationDate { get; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    private DateTime EvaluationDate { get; }

    /// <summary>
    /// The payroll repository
    /// </summary>
    private IPayrollRepository PayrollRepository { get; }

    /// <summary>
    /// Constructor for national case value provider
    /// </summary>
    public CaseProvider(Tenant tenant,
        IPayrollRepository payrollRepository,
        DateTime regulationDate,
        DateTime evaluationDate)
    {
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        PayrollRepository = payrollRepository ?? throw new ArgumentNullException(nameof(payrollRepository));
        RegulationDate = regulationDate.IsUtc()
            ? evaluationDate
            : throw new ArgumentException("Regulation date must be UTC", nameof(regulationDate));
        EvaluationDate = evaluationDate.IsUtc()
            ? evaluationDate
            : throw new ArgumentException("Evaluation date must be UTC", nameof(evaluationDate));
    }

    /// <inheritdoc />
    public async Task<Case> GetCaseAsync(IDbContext context, int payrollId, string caseName,
        OverrideType? overrideType = null, ClusterSet clusterSet = null)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }
        return (await PayrollRepository.GetDerivedCasesAsync(context,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = payrollId,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            null, new[] { caseName }, overrideType, clusterSet)).FirstOrDefault();
    }
}