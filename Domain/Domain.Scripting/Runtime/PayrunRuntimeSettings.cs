using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class PayrunRuntimeSettings : PayrollRuntimeSettings
{
    /// <summary>The Payrun</summary>
    public Payrun Payrun { get; init; }

    /// <summary>
    /// Provider for regulation items
    /// </summary>
    public IRegulationProvider RegulationProvider { get; init; }

    /// <summary>
    /// Provider for employee results
    /// </summary>
    public IResultProvider ResultProvider { get; init; }

    /// <summary>
    /// Provider for runtime values
    /// </summary>
    public IRuntimeValueProvider RuntimeValueProvider { get; init; }

    /// <summary>The Payrun job</summary>
    public PayrunJob PayrunJob { get; init; }

    /// <summary>The parent payrun job, usually the payrun retro source payrun job</summary>
    public PayrunJob ParentPayrunJob { get; init; }

    /// <summary>The parent payrun job, usually the payrun retro source payrun job</summary>
    public PayrunExecutionPhase ExecutionPhase { get; init; }
}