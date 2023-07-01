using System;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

internal sealed class CaseSetupSettings
{
    internal IDbContext DbContext { get; init; }
    internal Tenant Tenant { get; init; }
    internal string Culture { get; init; }
    internal Calendar Calendar { get; init; }
    internal Payroll Payroll { get; init; }
    internal User User { get; init; }
    internal Employee Employee { get; init; }
    internal CaseType CaseType { get; init; }
    internal IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }
    internal DateTime RegulationDate { get; init; }
    internal DateTime EvaluationDate { get; init; }
    internal string ClusterSetName { get; init; }
}