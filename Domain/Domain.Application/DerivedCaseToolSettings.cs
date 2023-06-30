using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Setting for the <see cref="DerivedCaseTool"/>
/// </summary>
public class DerivedCaseToolSettings : FunctionToolSettings
{
    public string ClusterSetName { get; set; }
    public Tenant Tenant { get; set; }
    public Calendar Calendar { get; set; }
    public User User { get; set; }
    public Payroll Payroll { get; set; }
    public Division Division { get; set; }
    public DateTime RegulationDate { get; set; }
    public DateTime EvaluationDate { get; set; }
    public IPayrollRepository PayrollRepository { get; set; }
    public IRegulationRepository RegulationRepository { get; set; }
    public ILookupSetRepository LookupSetRepository { get; set; }
    public IWebhookDispatchService WebhookDispatchService { get; set; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; set; }
}