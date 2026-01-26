using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Utils;

namespace DerotMyBrain.Core.Services;

public class UserFocusService : IUserFocusService
{
    private readonly IUserFocusRepository _repository;
    private readonly IActivityRepository _activityRepository;

    public UserFocusService(IUserFocusRepository repository, IActivityRepository activityRepository)
    {
        _repository = repository;
        _activityRepository = activityRepository;
    }

    public async Task<UserFocus> TrackTopicAsync(string userId, string sourceId, SourceType sourceType, string displayTitle)
    {
        var technicalSourceId = SourceHasher.GenerateHash(sourceType, sourceId);
        
        // Ensure Source exists (similar to ActivityService)
        var source = await _activityRepository.GetSourceByIdAsync(technicalSourceId);
        if (source == null)
        {
            source = new Source
            {
                Id = technicalSourceId,
                Type = sourceType,
                ExternalId = sourceId,
                DisplayTitle = displayTitle,
                Url = sourceType == SourceType.Wikipedia ? $"https://en.wikipedia.org/wiki/{sourceId}" : sourceId
            };
            await _activityRepository.CreateSourceAsync(source);
        }

        var existing = await _repository.GetBySourceIdAsync(userId, technicalSourceId);
        if (existing != null) return existing;

        var focus = new UserFocus
        {
            UserId = userId,
            SourceId = technicalSourceId,
            DisplayTitle = displayTitle,
            LastAttemptDate = DateTime.UtcNow
        };

        // Create initial entity
        await _repository.CreateAsync(focus);
        
        // Build stats from history
        await RebuildStatsAsync(userId, technicalSourceId);
        
        return (await _repository.GetBySourceIdAsync(userId, technicalSourceId))!;
    }

    public async Task UntrackTopicAsync(string userId, string sourceId)
    {
        var existing = await _repository.GetBySourceIdAsync(userId, sourceId);
        if (existing != null)
        {
            await _repository.DeleteAsync(existing.Id);
        }
    }

    public async Task<IEnumerable<UserFocus>> GetAllFocusesAsync(string userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<UserFocus?> GetFocusAsync(string userId, string sourceId)
    {
        return await _repository.GetBySourceIdAsync(userId, sourceId);
    }

    public async Task RebuildStatsAsync(string userId, string sourceId)
    {
        var focus = await _repository.GetBySourceIdAsync(userId, sourceId);
        if (focus == null) return;

        var activities = await _activityRepository.GetAllForContentAsync(userId, sourceId);
        
        focus.BestScore = 0;
        focus.LastScore = 0;
        focus.TotalReadTimeSeconds = 0;
        focus.TotalQuizTimeSeconds = 0;
        focus.TotalStudyTimeSeconds = 0;
        focus.LastAttemptDate = DateTime.MinValue;

        foreach (var activity in activities)
        {
            var activityDate = activity.SessionDateEnd ?? activity.SessionDateStart;
            if (activityDate > focus.LastAttemptDate)
            {
                focus.LastAttemptDate = activityDate;
                if (activity.ScorePercentage.HasValue)
                {
                    focus.LastScore = activity.ScorePercentage.Value;
                }
            }

            if (activity.ScorePercentage.HasValue && activity.ScorePercentage > focus.BestScore)
            {
                focus.BestScore = activity.ScorePercentage.Value;
            }

            focus.TotalReadTimeSeconds += (activity.ReadDurationSeconds ?? 0);
            focus.TotalQuizTimeSeconds += (activity.QuizDurationSeconds ?? 0);
            focus.TotalStudyTimeSeconds += activity.TotalDurationSeconds;
        }

        await _repository.UpdateAsync(focus);
    }

    public async Task UpdateStatsAsync(string userId, string sourceId, UserActivity activity)
    {
        var focus = await _repository.GetBySourceIdAsync(userId, sourceId);
        if (focus == null) return;

        var activityDate = activity.SessionDateEnd ?? activity.SessionDateStart;
        if (activityDate > focus.LastAttemptDate)
        {
            focus.LastAttemptDate = activityDate;
            if (activity.ScorePercentage.HasValue)
            {
                focus.LastScore = activity.ScorePercentage.Value;
            }
        }

        if (activity.ScorePercentage.HasValue && activity.ScorePercentage > focus.BestScore)
        {
            focus.BestScore = activity.ScorePercentage.Value;
        }

        focus.TotalReadTimeSeconds += (activity.ReadDurationSeconds ?? 0);
        focus.TotalQuizTimeSeconds += (activity.QuizDurationSeconds ?? 0);
        focus.TotalStudyTimeSeconds += activity.TotalDurationSeconds;

        await _repository.UpdateAsync(focus);
    }

    public async Task<UserFocus?> TogglePinAsync(string userId, string sourceId)
    {
        var focus = await _repository.GetBySourceIdAsync(userId, sourceId);
        if (focus == null) return null;

        focus.IsPinned = !focus.IsPinned;
        await _repository.UpdateAsync(focus);
        return focus;
    }

    public async Task<UserFocus?> ToggleArchiveAsync(string userId, string sourceId)
    {
        var focus = await _repository.GetBySourceIdAsync(userId, sourceId);
        if (focus == null) return null;

        focus.IsArchived = !focus.IsArchived;
        await _repository.UpdateAsync(focus);
        return focus;
    }
}
