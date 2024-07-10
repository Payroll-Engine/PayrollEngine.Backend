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
}