using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class CaseRuntimeSettings : PayrollRuntimeSettings
{
    /// <summary>The case</summary>
    public Case Case { get; set; }

    /// <summary>The webhook dispatch service</summary>
    public IWebhookDispatchService WebhookDispatchService { get; set; }
}