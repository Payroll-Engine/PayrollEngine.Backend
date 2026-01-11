using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application.Service;

/// <summary>
/// Represents a payrun job queued for background processing
/// </summary>
public class PayrunJobQueueItem
{
    /// <summary>
    /// The tenant ID
    /// </summary>
    public int TenantId { get; init; }

    /// <summary>
    /// The created payrun job ID
    /// </summary>
    public int PayrunJobId { get; init; }

    /// <summary>
    /// The tenant entity
    /// </summary>
    public Tenant Tenant { get; init; }

    /// <summary>
    /// The payrun entity
    /// </summary>
    public Payrun Payrun { get; init; }

    /// <summary>
    /// The original job invocation request
    /// </summary>
    public PayrunJobInvocation JobInvocation { get; init; }
}
