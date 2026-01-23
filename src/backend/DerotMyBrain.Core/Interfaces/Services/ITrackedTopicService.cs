using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface ITrackedTopicService
{
    Task<TrackedTopic> TrackTopicAsync(string userId, string topic, string wikipediaUrl);
    Task UntrackTopicAsync(string userId, string topic);
    Task<IEnumerable<TrackedTopic>> GetAllTrackedTopicsAsync(string userId);
    Task<TrackedTopic?> GetTrackedTopicAsync(string userId, string topic);
    Task UpdateStatsAsync(string userId, string topic, UserActivity activity);
}
