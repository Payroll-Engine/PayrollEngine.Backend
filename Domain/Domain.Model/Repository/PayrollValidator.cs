using System;
using System.Linq;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>Validator for <see cref="Payroll"/></summary>
public class PayrollValidator
{
    /// <summary>Gets the payroll repository</summary>
    private IPayrollRepository PayrollRepository { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollValidator"/> class.
    /// </summary>
    /// <param name="payrollRepository">The payroll repository></param>
    public PayrollValidator(IPayrollRepository payrollRepository)
    {
        PayrollRepository = payrollRepository ?? throw new ArgumentNullException(nameof(payrollRepository));
    }

    /// <summary>Validate payroll regulations</summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payroll">The payroll to validate</param>
    /// <param name="regulationDate">The regulation date</param>
    /// <param name="evaluationDate">The evaluation date</param>
    public async Task<string> ValidateRegulations(IDbContext context, int tenantId, Payroll payroll,
        DateTime regulationDate, DateTime evaluationDate)
    {
        if (payroll == null)
        {
            throw new ArgumentNullException(nameof(payroll));
        }

        // base regulations
        var regulations = (await PayrollRepository.GetDerivedRegulationsAsync(context,
            new()
            {
                TenantId = tenantId,
                PayrollId = payroll.Id,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            })).ToList();
        foreach (var regulation in regulations)
        {
            // no base regulations
            if (regulation.BaseRegulations == null || !regulation.BaseRegulations.Any())
            {
                continue;
            }

            var currentIndex = regulations.IndexOf(regulation);

            // test all regulations
            foreach (var baseRegulation in regulation.BaseRegulations)
            {
                var reference = new RegulationReference(baseRegulation);
                var referenceRegulation = regulations.FirstOrDefault(reg => reference.IsMatching(reg.Name, reg.Version));
                if (referenceRegulation == null)
                {
                    return $"Missing base regulation {baseRegulation}";
                }

                // test derived level
                var referenceIndex = regulations.IndexOf(referenceRegulation);
                if (referenceIndex < currentIndex)
                {
                    return $"Misplaced base regulation {baseRegulation} at position {referenceIndex}";
                }
            }
        }

        return null;
    }
}