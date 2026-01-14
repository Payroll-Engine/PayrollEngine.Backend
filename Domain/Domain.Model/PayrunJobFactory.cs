using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Factory for payrun jobs
/// </summary>
public static class PayrunJobFactory
{
    /// <summary>
    /// Create a new news payrun job
    /// </summary>
    /// <param name="jobInvocation">The job invocation</param>
    /// <param name="divisionId">The division id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="payrollCalculator">The payroll calculator</param>
    public static PayrunJob CreatePayrunJob(PayrunJobInvocation jobInvocation, int divisionId, int payrollId,
        IPayrollCalculator payrollCalculator)
    {
        // evaluation date: treat undefined as now
        if (!jobInvocation.EvaluationDate.HasValue || jobInvocation.EvaluationDate.Value == Date.MinValue)
        {
            jobInvocation.EvaluationDate = Date.Now;
        }
        // fix local times
        if (!jobInvocation.EvaluationDate.Value.IsUtc())
        {
            jobInvocation.EvaluationDate = DateTime.SpecifyKind(jobInvocation.EvaluationDate.Value, DateTimeKind.Utc);
        }
        if (!jobInvocation.PeriodStart.IsUtc())
        {
            jobInvocation.PeriodStart = DateTime.SpecifyKind(jobInvocation.PeriodStart, DateTimeKind.Utc);
        }

        var evaluationCycle = payrollCalculator.GetPayrunCycle(jobInvocation.PeriodStart);
        var evaluationPeriod = payrollCalculator.GetPayrunPeriod(jobInvocation.PeriodStart);
        var payrunJob = new PayrunJob
        {
            // invocation
            PayrunId = jobInvocation.PayrunId,
            PayrollId = payrollId,
            DivisionId = divisionId,
            CreatedUserId = jobInvocation.UserId,
            ParentJobId = jobInvocation.ParentJobId,
            Name = jobInvocation.Name,
            Owner = jobInvocation.Owner,
            Tags = jobInvocation.Tags,
            Forecast = jobInvocation.Forecast,
            RetroPayMode = jobInvocation.RetroPayMode,
            JobResult = jobInvocation.JobResult,
            Attributes = jobInvocation.Attributes,

            CycleName = evaluationCycle.Name,
            CycleStart = evaluationCycle.Start,
            CycleEnd = evaluationCycle.End,
            PeriodName = evaluationPeriod.Name,
            PeriodStart = evaluationPeriod.Start,
            PeriodEnd = evaluationPeriod.End,
            EvaluationDate = jobInvocation.EvaluationDate.Value,
            CreatedReason = jobInvocation.Reason,
            // runtime
            Employees = [],
            JobStatus = string.IsNullOrWhiteSpace(jobInvocation.Forecast) ? jobInvocation.JobStatus : PayrunJobStatus.Forecast
        };

        return payrunJob;
    }

    /// <summary>
    /// Update an existing payrun job with calculated values.
    /// Used for async processing when the job was pre-created by the controller.
    /// </summary>
    /// <param name="payrunJob">The existing payrun job to update</param>
    /// <param name="jobInvocation">The job invocation</param>
    /// <param name="divisionId">The division id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="payrollCalculator">The payroll calculator</param>
    public static void UpdatePayrunJob(PayrunJob payrunJob, PayrunJobInvocation jobInvocation,
        int divisionId, int payrollId, IPayrollCalculator payrollCalculator)
    {
        if (payrunJob == null)
        {
            throw new ArgumentNullException(nameof(payrunJob));
        }

        // evaluation date: treat undefined as now
        if (!jobInvocation.EvaluationDate.HasValue || jobInvocation.EvaluationDate.Value == Date.MinValue)
        {
            jobInvocation.EvaluationDate = Date.Now;
        }
        // fix local times
        if (!jobInvocation.EvaluationDate.Value.IsUtc())
        {
            jobInvocation.EvaluationDate = DateTime.SpecifyKind(jobInvocation.EvaluationDate.Value, DateTimeKind.Utc);
        }
        if (!jobInvocation.PeriodStart.IsUtc())
        {
            jobInvocation.PeriodStart = DateTime.SpecifyKind(jobInvocation.PeriodStart, DateTimeKind.Utc);
        }

        var evaluationCycle = payrollCalculator.GetPayrunCycle(jobInvocation.PeriodStart);
        var evaluationPeriod = payrollCalculator.GetPayrunPeriod(jobInvocation.PeriodStart);

        // Update calculated cycle/period values
        payrunJob.CycleName = evaluationCycle.Name;
        payrunJob.CycleStart = evaluationCycle.Start;
        payrunJob.CycleEnd = evaluationCycle.End;
        payrunJob.PeriodName = evaluationPeriod.Name;
        payrunJob.PeriodStart = evaluationPeriod.Start;
        payrunJob.PeriodEnd = evaluationPeriod.End;
        payrunJob.EvaluationDate = jobInvocation.EvaluationDate.Value;

        // Ensure division and payroll IDs are correct
        payrunJob.DivisionId = divisionId;
        payrunJob.PayrollId = payrollId;
    }
}