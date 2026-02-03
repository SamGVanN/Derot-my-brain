namespace DerotMyBrain.Core.Interfaces.Services;

/// <summary>
/// Queue for managing asynchronous content extraction jobs.
/// </summary>
public interface IContentExtractionQueue
{
    /// <summary>
    /// Adds a source ID to the extraction queue.
    /// </summary>
    /// <param name="sourceId">The ID of the source to extract content from.</param>
    void QueueExtraction(string sourceId);
    
    /// <summary>
    /// Dequeues the next source ID for extraction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next source ID to process, or null if queue is empty.</returns>
    ValueTask<string?> DequeueAsync(CancellationToken cancellationToken);
}
