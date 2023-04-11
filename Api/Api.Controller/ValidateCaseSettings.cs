using System;

namespace PayrollEngine.Api.Controller;

public class ValidateCaseSettings
{
    public Domain.Model.Tenant Tenant { get; set; }
    public Domain.Model.Payroll Payroll { get; set; }
    public Domain.Model.User User { get; set; }
    public Domain.Model.Employee Employee { get; set; }
    public Domain.Model.Case ValidationCase { get; set; }
    public CaseType CaseType { get; set; }
    public Domain.Model.CaseChangeSetup DomainCaseChangeSetup { get; set; }
    public DateTime? EvaluationDate { get; set; }
    public DateTime? RegulationDate { get; set; }
    public DateTime? CancellationDate { get; set; }
}