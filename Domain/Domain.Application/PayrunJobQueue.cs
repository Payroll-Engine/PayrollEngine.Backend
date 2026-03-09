using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Background queue for payrun job processing using System.Threading.Channels.
/// Uses a bounded channel to prevent unbounded memory growth under sustained load.
/// </summary>
public class PayrunJobQueue : IPayrunJobQueue
{
    /// <summary>
    /// Default maximum number of queued jobs
    /// </summary>
    private const int DefaultCapacity = 100;

    private readonly Channel<PayrunJobQueueItem> queue;

    /// <summary>
    /// Initializes a new instance of the PayrunJobQueue with default capacity
    /// </summary>
    public PayrunJobQueue() :
        this(DefaultCapacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the PayrunJobQueue
    /// </summary>
    /// <param name="capacity">Maximum number of queued jobs (must be greater than zero)</param>
    public PayrunJobQueue(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity,
                "Queue capacity must be greater than zero.");
        }

        var options = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,   // only one worker reads
            SingleWriter = false,  // multiple requests can write
            FullMode = BoundedChannelFullMode.DropWrite // TryWrite returns false when full
        };
        queue = Channel.CreateBounded<PayrunJobQueueItem>(options);
    }

    /// <inheritdoc />
    public ValueTask EnqueueAsync(PayrunJobQueueItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!queue.Writer.TryWrite(item))
        {
            throw new InvalidOperationException(
                "The payrun job queue is full. The server cannot accept new jobs at this time. Please retry later.");
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<PayrunJobQueueItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return await queue.Reader.ReadAsync(cancellationToken);
    }
}
