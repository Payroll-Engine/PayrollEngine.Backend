using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Api.Controller;

internal sealed class CaseSetupSettings
{
    internal IDbContext DbContext { get; init; }
    internal Tenant Tenant { get; init; }
    internal Payroll Payroll { get; init; }
    internal User User { get; init; }
    internal Employee Employee { get; init; }
    internal CaseType CaseType { get; init; }
    internal RegulationLookupProvider RegulationLookupProvider { get; init; }
    internal DateTime RegulationDate { get; init; }
    internal DateTime EvaluationDate { get; init; }
    internal string ClusterSetName { get; init; }
}