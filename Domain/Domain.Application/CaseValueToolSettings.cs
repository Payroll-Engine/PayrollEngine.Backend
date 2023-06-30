using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Setting for the <see cref="CaseValueTool"/>
/// </summary>
public class CaseValueToolSettings : FunctionToolSettings
{
    public Tenant Tenant { get; init; }
    public Calendar Calendar { get; init; }
    public Payroll Payroll { get; init; }
    public DateTime ValueDate { get; init; }
    public DateTime EvaluationDate { get; init; }
    public IPayrollRepository PayrollRepository { get; init; }
    public ICaseRepository CaseRepository { get; init; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }
}