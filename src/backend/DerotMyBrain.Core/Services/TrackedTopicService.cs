using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;

namespace DerotMyBrain.Core.Services;

public class TrackedTopicService : ITrackedTopicService
{
    private readonly ITrackedTopicRepository _repository;
    private readonly IActivityRepository _activityRepository;

    public TrackedTopicService(ITrackedTopicRepository repository, IActivityRepository activityRepository)
    {
        _repository = repository;
        _activityRepository = activityRepository;
    }

    public async Task<TrackedTopic> TrackTopicAsync(string userId, string topic, string wikipediaUrl)
    {
        var existing = await _repository.GetByTopicAsync(userId, topic);
        if (existing != null) return existing;

        var tracked = new TrackedTopic
        {
            UserId = userId,
            Name = topic,
            LastInteraction = DateTime.UtcNow,
            InteractionCount = 0,
            MasteryLevel = 0
            // Url could be stored if we added it to entity, currently not in entity
        };

        // Rebuild history
        var activities = await _activityRepository.GetAllForTopicAsync(userId, topic);
        foreach (var activity in activities)
        {
            tracked.InteractionCount++;
            if (activity.LastAttemptDate > tracked.LastInteraction)
                tracked.LastInteraction = activity.LastAttemptDate;
            // Simple mastery logic
            if (activity.Type == "Quiz" && activity.Percentage > 80)
                tracked.MasteryLevel = Math.Min(100, tracked.MasteryLevel + 10);
        }

        return await _repository.CreateAsync(tracked);
    }

    public async Task UntrackTopicAsync(string userId, string topic)
    {
        var existing = await _repository.GetByTopicAsync(userId, topic);
        if (existing != null)
        {
            await _repository.DeleteAsync(existing.Id);
        }
    }

    public async Task<IEnumerable<TrackedTopic>> GetAllTrackedTopicsAsync(string userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<TrackedTopic?> GetTrackedTopicAsync(string userId, string topic)
    {
        return await _repository.GetByTopicAsync(userId, topic);
    }
}
