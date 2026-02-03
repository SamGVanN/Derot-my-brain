using System.Collections.Generic;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Utils;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Core.Services;

public class BacklogService : IBacklogService
{
    private readonly IBacklogRepository _backlogRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ISourceService _sourceService;
    private readonly ILogger<BacklogService> _logger;

    public BacklogService(
        IBacklogRepository backlogRepository, 
        IActivityRepository activityRepository,
        ISourceService sourceService,
        ILogger<BacklogService> logger)
    {
        _backlogRepository = backlogRepository;
        _activityRepository = activityRepository;
        _sourceService = sourceService;
        _logger = logger;
    }

    public async Task<BacklogItem> AddToBacklogAsync(string userId, string sourceId, SourceType sourceType, string title, string? url = null, string? provider = null)
    {
        Source? source = null;
        string? resourceId = null;

        // 1. Resolve Technical Hub (Source)
        if (sourceType == SourceType.Document)
        {
            // For Documents, SourceId is the Guid
            source = await _activityRepository.GetSourceByIdAsync(sourceId);
            if (source == null)
            {
                 // Handle new doc (should not happen for backlog usually as it's uploaded first)
                source = new Source
                {
                    Id = sourceId, // GUID
                    UserId = userId,
                    Type = sourceType,
                    ExternalId = sourceId,
                    DisplayTitle = title
                };
                await _activityRepository.CreateSourceAsync(source);
            }
        }
        else // Wikipedia or other Web Content
        {
            string targetUrl = url;
            if (string.IsNullOrEmpty(targetUrl) && sourceType == SourceType.Wikipedia)
            {
                targetUrl = sourceId.StartsWith("http") ? sourceId : $"https://en.wikipedia.org/wiki/{sourceId}";
            }
            
            if (!string.IsNullOrEmpty(targetUrl))
            {
                resourceId = SourceHasher.GenerateId(sourceType, targetUrl); // Hash of the URL
                
                // Lookup existing Source Hub for this content
                source = await _activityRepository.GetSourceByExternalIdAsync(userId, resourceId);

                if (source == null)
                {
                    // Create new Source Hub
                    source = new Source
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Type = sourceType,
                        ExternalId = resourceId,
                        DisplayTitle = title
                    };
                    await _activityRepository.CreateSourceAsync(source);

                    // Ensure OnlineResource exists
                    var onlineResource = await _activityRepository.GetOnlineResourceByIdAsync(resourceId);
                    if (onlineResource == null)
                    {
                        onlineResource = new OnlineResource
                        {
                            Id = resourceId,
                            UserId = userId,
                            SourceId = source.Id, 
                            URL = targetUrl,
                            Title = title,
                            Provider = provider ?? (sourceType == SourceType.Wikipedia ? "Wikipedia" : null),
                            SavedAt = DateTime.UtcNow
                        };
                        await _activityRepository.CreateOnlineResourceAsync(onlineResource);
                    }
                    source.OnlineResource = onlineResource;
                }
            }
            else 
            {
                // Fallback: search by title or throw? Plan said to use Id.
                source = await _activityRepository.GetSourceByIdAsync(sourceId);
                if (source == null) throw new ArgumentException("Cannot resolve source for backlog without URL or valid ID.");
            }
        }

        // Check if already exists in backlog to ensure idempotency
        if (source.IsInBacklog)
        {
            _logger.LogInformation("Source {SourceId} (Title: {Title}) already in backlog for user {UserId}", source.Id, title, userId);
            // If BacklogItem is missing but IsInBacklog is true, fix it
            var existingItem = await _backlogRepository.GetBySourceIdAsync(userId, source.Id);
            if (existingItem != null) return existingItem;
        }

        // Set flag on source
        source.IsInBacklog = true;
        await _activityRepository.UpdateSourceAsync(source);

        var item = new BacklogItem
        {
            UserId = userId,
            SourceId = source.Id,
            Title = title,
            AddedAt = DateTime.UtcNow
        };

        var bItem = await _backlogRepository.CreateAsync(item);
        
        // Populate content in background or sequentially (centralized logic)
        await _sourceService.PopulateSourceContentAsync(source);
        
        return bItem;
    }

    public async Task RemoveFromBacklogAsync(string userId, string sourceId)
    {
        await _backlogRepository.DeleteAsync(userId, sourceId);
        
        // Update flag on source
        var source = await _activityRepository.GetSourceByIdAsync(sourceId);
        if (source != null)
        {
            source.IsInBacklog = false;
            await _activityRepository.UpdateSourceAsync(source);
            
            // Also remove source if it's not tracked and has no activities (Cleanup as requested)
            if (!source.IsTracked && (source.Activities == null || !source.Activities.Any()))
            {
                await _activityRepository.DeleteSourceAsync(sourceId);
            }
        }
    }

    public async Task<IEnumerable<BacklogItem>> GetUserBacklogAsync(string userId)
    {
        return await _backlogRepository.GetAllAsync(userId);
    }

    public async Task<bool> IsInBacklogAsync(string userId, string sourceId)
    {
        var item = await _backlogRepository.GetBySourceIdAsync(userId, sourceId);
        return item != null;
    }
}
