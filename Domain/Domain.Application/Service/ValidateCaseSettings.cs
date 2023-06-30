using System;

namespace PayrollEngine.Domain.Application.Service;

public class ValidateCaseSettings
{
    public Model.Tenant Tenant { get; init; }
    public Model.Payroll Payroll { get; init; }
    public Model.Division Division { get; init; }
    public Model.User User { get; init; }
    public Model.Employee Employee { get; init; }
    public Model.Case ValidationCase { get; init; }
    public CaseType CaseType { get; init; }
    public Model.CaseChangeSetup DomainCaseChangeSetup { get; init; }
    public DateTime? EvaluationDate { get; set; }
    public DateTime? RegulationDate { get; set; }
    public DateTime? CancellationDate { get; init; }
}