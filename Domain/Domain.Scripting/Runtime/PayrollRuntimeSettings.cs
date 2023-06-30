using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class PayrollRuntimeSettings : RuntimeSettings
{
    /// <summary>
    /// The payroll
    /// </summary>
    public Payroll Payroll { get; init; }

    /// <summary>
    /// The case value provider
    /// </summary>
    public ICaseValueProvider CaseValueProvider { get; init; }

    /// <summary>
    /// Provider for regulation lookups
    /// </summary>
    public IRegulationLookupProvider RegulationLookupProvider { get; init; }
}