using System;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

public class CaseValueProviderSettings
{
    public IDbContext DbContext { get; init; }
    public IPayrollCalculator Calculator { get; init; }
    public ICaseFieldProvider CaseFieldProvider { get; init; }
    public DatePeriod EvaluationPeriod { get; init; }
    public DateTime EvaluationDate { get; init; }
    public DateTime? RetroDate { get; init; }
}