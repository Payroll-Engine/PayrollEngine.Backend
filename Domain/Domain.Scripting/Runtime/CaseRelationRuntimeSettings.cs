using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class CaseRelationRuntimeSettings : PayrollRuntimeSettings
{
    /// <summary>
    /// The case values of the relation source
    /// </summary>
    public CaseSet SourceCaseSet { get; set; }

    /// <summary>
    /// The case values of the relation target
    /// </summary>
    public CaseSet TargetCaseSet { get; set; }

    /// <summary>The webhook dispatch service</summary>
    public IWebhookDispatchService WebhookDispatchService { get; set; }
}