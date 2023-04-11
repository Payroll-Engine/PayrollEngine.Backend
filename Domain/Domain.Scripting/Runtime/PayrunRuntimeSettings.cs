using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class PayrunRuntimeSettings : PayrollRuntimeSettings
{
    /// <summary>The webhook dispatch service</summary>
    public IWebhookDispatchService WebhookDispatchService { get; set; }

    /// <summary>The Payrun</summary>
    public Payrun Payrun { get; set; }

    /// <summary>
    /// Provider for employee results
    /// </summary>
    public ResultProvider ResultProvider { get; set; }

    /// <summary>
    /// Provider for runtime values
    /// </summary>
    public RuntimeValueProvider RuntimeValueProvider { get; set; }

    /// <summary>The Payrun job</summary>
    public PayrunJob PayrunJob { get; set; }

    /// <summary>The parent payrun job, usually the payrun retro source payrun job</summary>
    public PayrunJob ParentPayrunJob { get; set; }

    /// <summary>The parent payrun job, usually the payrun retro source payrun job</summary>
    public PayrunExecutionPhase ExecutionPhase { get; set; }
}