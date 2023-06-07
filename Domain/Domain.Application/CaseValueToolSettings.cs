using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Setting for the <see cref="CaseValueTool"/>
/// </summary>
public class CaseValueToolSettings : FunctionToolSettings
{
    public Tenant Tenant { get; set; }
    public Payroll Payroll { get; set; }
    public DateTime ValueDate { get; set; }
    public DateTime EvaluationDate { get; set; }
    public IPayrollRepository PayrollRepository { get; set; }
    public ICaseRepository CaseRepository { get; set; }
    public IRegulationLookupProvider RegulationLookupProvider { get; set; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; set; }
}