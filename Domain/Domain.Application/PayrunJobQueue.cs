using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Background queue for payrun job processing using System.Threading.Channels
/// </summary>
public class PayrunJobQueue : IPayrunJobQueue
{
    private readonly Channel<PayrunJobQueueItem> _queue;

    /// <summary>
    /// Initializes a new instance of the PayrunJobQueue
    /// </summary>
    public PayrunJobQueue()
    {
        // Unbounded channel - jobs will queue without blocking
        // This is appropriate since job creation is already validated
        var options = new UnboundedChannelOptions
        {
            SingleReader = true,  // Only one worker reads
            SingleWriter = false  // Multiple requests can write
        };
        _queue = Channel.CreateUnbounded<PayrunJobQueueItem>(options);
    }

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(PayrunJobQueueItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        await _queue.Writer.WriteAsync(item);
    }

    /// <inheritdoc />
    public async ValueTask<PayrunJobQueueItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
