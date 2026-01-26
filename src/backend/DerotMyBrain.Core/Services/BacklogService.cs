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
    private readonly ILogger<BacklogService> _logger;

    public BacklogService(IBacklogRepository backlogRepository, ILogger<BacklogService> logger)
    {
        _backlogRepository = backlogRepository;
        _logger = logger;
    }

    public async Task<BacklogItem> AddToBacklogAsync(string userId, string sourceId, SourceType sourceType, string title)
    {
        var technicalSourceId = SourceHasher.GenerateId(sourceType, sourceId);

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
