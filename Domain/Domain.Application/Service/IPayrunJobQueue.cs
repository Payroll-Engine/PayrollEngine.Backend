using System.Threading;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Application.Service;

/// <summary>
/// Interface for the payrun job background processing queue
/// </summary>
public interface IPayrunJobQueue
{
    /// <summary>
    /// Enqueue a payrun job for background processing
    /// </summary>
    /// <param name="item">The queue item containing job details</param>
    ValueTask EnqueueAsync(PayrunJobQueueItem item);

    /// <summary>
    /// Dequeue a payrun job for processing
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The next job to process</returns>
    ValueTask<PayrunJobQueueItem> DequeueAsync(CancellationToken cancellationToken);
}
