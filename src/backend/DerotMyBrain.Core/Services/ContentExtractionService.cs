using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Core.Services;

/// <summary>
/// Background service that processes document content extraction jobs from a queue.
/// </summary>
public class ContentExtractionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IContentExtractionQueue _queue;
    private readonly ILogger<ContentExtractionService> _logger;

    public ContentExtractionService(
        IServiceScopeFactory scopeFactory,
        IContentExtractionQueue queue,
        ILogger<ContentExtractionService> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Content Extraction Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var sourceId = await _queue.DequeueAsync(stoppingToken);
                
                if (sourceId != null)
                {
                    await ProcessExtractionAsync(sourceId, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Service is stopping, exit gracefully
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in content extraction service");
                // Continue processing other items
            }
        }

        _logger.LogInformation("Content Extraction Service stopped");
    }

    private async Task ProcessExtractionAsync(string sourceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting content extraction for source {SourceId}", sourceId);

        // Create a new scope for each extraction job to get fresh DbContext
        using var scope = _scopeFactory.CreateScope();
        var activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
        var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var textExtractor = scope.ServiceProvider.GetRequiredService<ITextExtractor>();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        try
        {
            // 1. Get the source and update status to Processing
            _logger.LogDebug("Retrieving source {SourceId} from repository", sourceId);
            var source = await activityRepository.GetSourceByIdAsync(sourceId);
            if (source == null)
            {
                _logger.LogWarning("Source {SourceId} not found for extraction", sourceId);
                return;
            }

            // Only process documents
            if (source.Type != SourceType.Document)
            {
                _logger.LogInformation("Source {SourceId} is not a document (Type: {Type}), skipping extraction", 
                    sourceId, source.Type);
                return;
            }

            if (source.ContentExtractionStatus == ContentExtractionStatus.Completed && !string.IsNullOrEmpty(source.TextContent))
            {
                _logger.LogInformation("Source {SourceId} already has extracted content and status is Completed, skipping", sourceId);
                return;
            }

            _logger.LogDebug("Updating source {SourceId} status to Processing", sourceId);
            source.ContentExtractionStatus = ContentExtractionStatus.Processing;
            await activityRepository.UpdateSourceAsync(source);
            _logger.LogInformation("Source {SourceId} status updated to Processing", sourceId);

            // 2. Find the associated document
            _logger.LogDebug("Retrieving document for source {SourceId}", sourceId);
            var document = await documentRepository.GetBySourceIdAsync(source.UserId, sourceId);
            if (document == null)
            {
                _logger.LogError("No document found for source {SourceId} (User: {UserId})", sourceId, source.UserId);
                throw new InvalidOperationException($"No document found for source {sourceId}");
            }

            // 3. Extract content from the file
            _logger.LogInformation("Extracting content from {FilePath} (Type: {FileType})", document.StoragePath, document.FileType);
            
            _logger.LogDebug("Opening file stream for {FilePath}", document.StoragePath);
            using var fileStream = await fileStorageService.GetFileStreamAsync(document.StoragePath);
            
            _logger.LogDebug("Calling text extractor for {FileType}", document.FileType);
            var extractedContent = textExtractor.ExtractText(fileStream, document.FileType);

            if (string.IsNullOrWhiteSpace(extractedContent))
            {
                _logger.LogWarning("Extracted content is empty for source {SourceId}", sourceId);
                // We keep going but mark it as completed (empty content is still "extracted")
                // Or you can throw to mark as failed. Let's decide based on requirements.
                // For now, let's allow empty content but log it.
            }

            _logger.LogDebug("Saving extracted content to source {SourceId} ({CharCount} chars)", sourceId, extractedContent?.Length ?? 0);

            // 4. Update source with extracted content
            source.TextContent = extractedContent ?? string.Empty;
            source.ContentExtractionStatus = ContentExtractionStatus.Completed;
            source.ContentExtractionCompletedAt = DateTime.UtcNow;
            source.ContentExtractionError = null; // Clear any previous errors
            
            await activityRepository.UpdateSourceAsync(source);

            _logger.LogInformation(
                "Content extraction completed successfully for source {SourceId}. Extracted {CharCount} characters",
                sourceId, extractedContent?.Length ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content extraction failed for source {SourceId}", sourceId);

            try
            {
                // Update source with error status
                var source = await activityRepository.GetSourceByIdAsync(sourceId);
                if (source != null)
                {
                    source.ContentExtractionStatus = ContentExtractionStatus.Failed;
                    source.ContentExtractionError = ex.Message;
                    source.ContentExtractionCompletedAt = DateTime.UtcNow;
                    await activityRepository.UpdateSourceAsync(source);
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update error status for source {SourceId}", sourceId);
            }
        }
    }
}
