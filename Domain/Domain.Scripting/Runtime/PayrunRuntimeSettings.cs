using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

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

    /// <summary>Test for preview payrun job</summary>
    public bool PreviewJob { get; init; }

    /// <summary>The parent payrun job, usually the payrun retro source payrun job</summary>
    public PayrunExecutionPhase ExecutionPhase { get; init; }

    /// <summary>
    /// Optional pre-loaded cycle cache for this employee.
    /// When non-null, <see cref="PayrunRuntimeBase"/> serves matching
    /// <c>GetWageTypeResults(cycleStart, previousPeriodEnd)</c> calls from memory
    /// instead of issuing a DB query. Works for any calendar cycle type.
    /// </summary>
    public WageTypeCycleCache WageTypeYtdCache { get; init; }

    /// <summary>
    /// Optional pre-loaded consolidated cache for this employee.
    /// When non-null, <see cref="PayrunRuntimeBase"/> serves matching
    /// <c>GetConsolidatedWageTypeResults(cycleStart, ...)</c> calls from memory
    /// instead of issuing a DB query.
    /// </summary>
    public WageTypeConsCache WageTypeConsCache { get; init; }
}