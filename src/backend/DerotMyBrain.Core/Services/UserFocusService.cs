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
        var sourceHash = SourceHasher.GenerateHash(sourceType, sourceId);
        var existing = await _repository.GetByHashAsync(userId, sourceHash);
        if (existing != null) return existing;

        var focus = new UserFocus
        {
            UserId = userId,
            SourceId = sourceId,
            SourceType = sourceType,
            SourceHash = sourceHash,
            DisplayTitle = displayTitle,
            LastAttemptDate = DateTime.UtcNow
        };

        // Create initial entity
        await _repository.CreateAsync(focus);
        
        // Build stats from history
        await RebuildStatsAsync(userId, sourceHash);
        
        return (await _repository.GetByHashAsync(userId, sourceHash))!;
    }

    public async Task UntrackTopicAsync(string userId, string sourceHash)
    {
        var existing = await _repository.GetByHashAsync(userId, sourceHash);
        if (existing != null)
        {
            await _repository.DeleteAsync(existing.Id);
        }
    }

    public async Task<IEnumerable<UserFocus>> GetAllFocusesAsync(string userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<UserFocus?> GetFocusAsync(string userId, string sourceHash)
    {
        return await _repository.GetByHashAsync(userId, sourceHash);
    }

    public async Task RebuildStatsAsync(string userId, string sourceHash)
    {
        var focus = await _repository.GetByHashAsync(userId, sourceHash);
        if (focus == null) return;

        var activities = await _activityRepository.GetAllForContentAsync(userId, sourceHash);
        
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

    public async Task UpdateStatsAsync(string userId, string sourceHash, UserActivity activity)
    {
        var focus = await _repository.GetByHashAsync(userId, sourceHash);
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
}
