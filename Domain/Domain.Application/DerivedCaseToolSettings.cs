using System;
using System.Globalization;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Setting for the <see cref="DerivedCaseTool"/>
/// </summary>
public class DerivedCaseToolSettings : FunctionToolSettings
{
    public Tenant Tenant { get; set; }
    public User User { get; set; }
    public Payroll Payroll { get; set; }
    public DateTime RegulationDate { get; set; }
    public DateTime EvaluationDate { get; set; }
    public IPayrollRepository PayrollRepository { get; set; }
    public ICaseRepository CaseRepository { get; set; }
    public ICaseRelationRepository CaseRelationRepository { get; set; }
    public IRegulationRepository RegulationRepository { get; set; }
    public ILookupSetRepository LookupSetRepository { get; set; }
    public IWebhookDispatchService WebhookDispatchService { get; set; }
    public RegulationLookupProvider RegulationLookupProvider { get; set; }
    public string ClusterSetName { get; set; }
    public CultureInfo Culture { get; set; }
}