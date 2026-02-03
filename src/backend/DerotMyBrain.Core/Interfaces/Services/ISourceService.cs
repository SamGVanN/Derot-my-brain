using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface ISourceService
{
    Task<IEnumerable<TrackedSourceDto>> GetTrackedSourcesAsync(string userId);
    Task<TrackedSourceDto?> GetTrackedSourceAsync(string userId, string sourceId);
    Task<Source?> GetSourceAsync(string sourceId);
    Task<Source> ToggleTrackingAsync(string userId, string sourceId, bool isTracked);
    Task<Source> TogglePinAsync(string userId, string sourceId);
    Task<Source> ToggleArchiveAsync(string userId, string sourceId);
    Task<Source> TrackSourceAsync(string userId, string sourceId, string title, SourceType type);
    Task<Source> GetOrCreateSourceAsync(string userId, string title, string sourceId, SourceType type);
    Task UpdateSourceAsync(Source source);
}
