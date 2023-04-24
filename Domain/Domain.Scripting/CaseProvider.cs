using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides a case
/// </summary>
public sealed class CaseProvider
{
    /// <summary>
    /// The tenant
    /// </summary>
    public Tenant Tenant { get; }

    /// <summary>
    /// The regulation date
    /// </summary>
    public DateTime RegulationDate { get; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    public DateTime EvaluationDate { get; }

    /// <summary>
    /// The payroll repository
    /// </summary>
    public IPayrollRepository PayrollRepository { get; }

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

    /// <summary>
    /// Get payroll cases
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseType">The case type (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The cases at a given time</returns>
    public async Task<IEnumerable<Case>> GetCasesAsync(IDbContext context, int payrollId, CaseType? caseType = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null) =>
        await PayrollRepository.GetDerivedCasesAsync(context,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = payrollId,
                RegulationDate = RegulationDate,
                EvaluationDate = EvaluationDate
            },
            caseType, null, overrideType, clusterSet);

    /// <summary>
    /// Get payroll case
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseName">The case name</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The case matching the name at a given time</returns>
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