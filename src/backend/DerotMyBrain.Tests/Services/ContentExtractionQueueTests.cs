using System;
using System.Threading;
using System.Threading.Tasks;
using DerotMyBrain.Core.Services;
using Xunit;

namespace DerotMyBrain.Tests.Services;

public class ContentExtractionQueueTests
{
    [Fact]
    public async Task QueueExtraction_ValidSourceId_AddsToQueue()
    {
        // Arrange
        var queue = new ContentExtractionQueue();
        var sourceId = "test-source-1";

        // Act
        queue.QueueExtraction(sourceId);

        // Assert - if we can dequeue it, it was added
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var result = await queue.DequeueAsync(cts.Token);
        Assert.Equal(sourceId, result);
    }

    [Fact]
    public void QueueExtraction_NullSourceId_ThrowsArgumentException()
    {
        // Arrange
        var queue = new ContentExtractionQueue();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => queue.QueueExtraction(null!));
    }

    [Fact]
    public void QueueExtraction_EmptySourceId_ThrowsArgumentException()
    {
        // Arrange
        var queue = new ContentExtractionQueue();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => queue.QueueExtraction(string.Empty));
    }

    [Fact]
    public async Task DequeueAsync_WithQueuedItem_ReturnsSourceId()
    {
        // Arrange
        var queue = new ContentExtractionQueue();
        var sourceId = "test-source-2";
        queue.QueueExtraction(sourceId);

        // Act
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var result = await queue.DequeueAsync(cts.Token);

        // Assert
        Assert.Equal(sourceId, result);
    }

    [Fact]
    public async Task DequeueAsync_EmptyQueue_WaitsForItem()
    {
        // Arrange
        var queue = new ContentExtractionQueue();
        var sourceId = "test-source-3";

        // Act - Start dequeue in background, then queue an item
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var dequeueTask = queue.DequeueAsync(cts.Token).AsTask();
        
        // Wait a bit to ensure dequeue is waiting
        await Task.Delay(100);
        
        // Now queue the item
        queue.QueueExtraction(sourceId);
        
        var result = await dequeueTask;

        // Assert
        Assert.Equal(sourceId, result);
    }

    [Fact]
    public async Task QueueExtraction_MultipleConcurrent_MaintainsOrder()
    {
        // Arrange
        var queue = new ContentExtractionQueue();
        var sourceIds = new[] { "source-1", "source-2", "source-3" };

        // Act - Queue multiple items
        foreach (var id in sourceIds)
        {
            queue.QueueExtraction(id);
        }

        // Dequeue and verify FIFO order
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var result1 = await queue.DequeueAsync(cts.Token);
        var result2 = await queue.DequeueAsync(cts.Token);
        var result3 = await queue.DequeueAsync(cts.Token);

        // Assert
        Assert.Equal("source-1", result1);
        Assert.Equal("source-2", result2);
        Assert.Equal("source-3", result3);
    }
}
