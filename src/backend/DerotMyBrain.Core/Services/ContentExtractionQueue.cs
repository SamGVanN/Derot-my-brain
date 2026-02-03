using System.Threading.Channels;
using DerotMyBrain.Core.Interfaces.Services;

namespace DerotMyBrain.Core.Services;

/// <summary>
/// Thread-safe queue for managing content extraction jobs using System.Threading.Channels.
/// </summary>
public class ContentExtractionQueue : IContentExtractionQueue
{
    private readonly Channel<string> _queue;

    public ContentExtractionQueue()
    {
        // Create an unbounded channel for simplicity
        // For production, consider bounded channel with capacity limits
        _queue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true, // Only one background service will read
            SingleWriter = false // Multiple upload requests can write
        });
    }

    public void QueueExtraction(string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            throw new ArgumentException("Source ID cannot be null or empty", nameof(sourceId));
        }

        var result = _queue.Writer.TryWrite(sourceId);
        if (!result)
        {
            // This should only happen if the channel is closed
            throw new InvalidOperationException("Failed to write to extraction queue. Channel might be closed.");
        }
    }

    public async ValueTask<string?> DequeueAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
        catch (ChannelClosedException)
        {
            return null;
        }
    }
}
