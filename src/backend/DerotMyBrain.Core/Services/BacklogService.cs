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
    private readonly ILogger<BacklogService> _logger;

    public BacklogService(
        IBacklogRepository backlogRepository, 
        IActivityRepository activityRepository,
        ILogger<BacklogService> logger)
    {
        _backlogRepository = backlogRepository;
        _activityRepository = activityRepository;
        _logger = logger;
    }

    public async Task<BacklogItem> AddToBacklogAsync(string userId, string sourceId, SourceType sourceType, string title, string? url = null, string? provider = null)
    {
        string technicalSourceId;
        if (sourceType == SourceType.Document)
        {
            technicalSourceId = sourceId;
        }
        else
        {
             technicalSourceId = SourceHasher.GenerateId(sourceType, sourceId);
        }

        // Ensure the Source entity exists because BacklogItem has a FK to it
        var source = await _activityRepository.GetSourceByIdAsync(technicalSourceId);
        if (source == null)
        {
            source = new Source
            {
                Id = technicalSourceId,
                UserId = userId,
                Type = sourceType,
                ExternalId = sourceId,
                DisplayTitle = title
            };
            await _activityRepository.CreateSourceAsync(source);
        }

        // Handle OnlineResource creation/update
        if (!string.IsNullOrEmpty(url))
        {
            var onlineResource = await _activityRepository.GetOnlineResourceBySourceIdAsync(technicalSourceId);
            if (onlineResource == null)
            {
                onlineResource = new OnlineResource
                {
                    UserId = userId,
                    SourceId = technicalSourceId,
                    URL = url,
                    Title = title,
                    Provider = provider,
                    SavedAt = DateTime.UtcNow
                };
                await _activityRepository.CreateOnlineResourceAsync(onlineResource);
            }
        }
        // Fallback for legacy Wikipedia flow where URL was inferred
        else if (sourceType == SourceType.Wikipedia && !string.IsNullOrEmpty(sourceId))
        {
             var inferredUrl = sourceId.StartsWith("http") ? sourceId : $"https://en.wikipedia.org/wiki/{sourceId}";
             var onlineResource = await _activityRepository.GetOnlineResourceBySourceIdAsync(technicalSourceId);
             if (onlineResource == null)
             {
                 onlineResource = new OnlineResource
                 {
                     UserId = userId,
                     SourceId = technicalSourceId,
                     URL = inferredUrl,
                     Title = title,
                     Provider = "Wikipedia",
                     SavedAt = DateTime.UtcNow
                 };
                 await _activityRepository.CreateOnlineResourceAsync(onlineResource);
             }
        }

        // Check if already exists to ensure idempotency
        var existing = await _backlogRepository.GetBySourceIdAsync(userId, technicalSourceId);
        if (existing != null)
        {
            _logger.LogInformation("Item {SourceId} (Title: {Title}) already in backlog for user {UserId}", technicalSourceId, title, userId);
            return existing;
        }

        var item = new BacklogItem
        {
            UserId = userId,
            SourceId = technicalSourceId,
            Title = title,
            AddedAt = DateTime.UtcNow
        };

        return await _backlogRepository.CreateAsync(item);
    }

    public async Task RemoveFromBacklogAsync(string userId, string sourceId)
    {
        await _backlogRepository.DeleteAsync(userId, sourceId);
        
        // Also remove source if it's not tracked and has no activities (Cleanup as requested)
        var source = await _activityRepository.GetSourceByIdAsync(sourceId);
        if (source != null && !source.IsTracked && (source.Activities == null || !source.Activities.Any()))
        {
            await _activityRepository.DeleteSourceAsync(sourceId);
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
