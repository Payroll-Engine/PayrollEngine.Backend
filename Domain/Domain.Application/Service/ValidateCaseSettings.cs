using System;

namespace PayrollEngine.Domain.Application.Service;

public class ValidateCaseSettings
{
    public Model.Tenant Tenant { get; set; }
    public Model.Payroll Payroll { get; set; }
    public Model.User User { get; set; }
    public Model.Employee Employee { get; set; }
    public Model.Case ValidationCase { get; set; }
    public CaseType CaseType { get; set; }
    public Model.CaseChangeSetup DomainCaseChangeSetup { get; set; }
    public DateTime? EvaluationDate { get; set; }
    public DateTime? RegulationDate { get; set; }
    public DateTime? CancellationDate { get; set; }
}