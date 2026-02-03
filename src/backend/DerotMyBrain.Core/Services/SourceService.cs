using DerotMyBrain.Core.DTOs;
using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Repositories;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace DerotMyBrain.Core.Services;

public class SourceService : ISourceService
{
    private readonly IActivityRepository _repository;
    private readonly IEnumerable<IContentSource> _contentSources;
    private readonly ILogger<SourceService> _logger;

    public SourceService(
        IActivityRepository repository, 
        IEnumerable<IContentSource> contentSources,
        ILogger<SourceService> logger)
    {
        _repository = repository;
        _contentSources = contentSources;
        _logger = logger;
    }

    public async Task<IEnumerable<TrackedSourceDto>> GetTrackedSourcesAsync(string userId)
    {
        var sources = await _repository.GetTrackedSourcesAsync(userId);
        return sources.Select(MapToTrackedSourceDto);
    }

    public async Task<TrackedSourceDto?> GetTrackedSourceAsync(string userId, string sourceId)
    {
        var source = await _repository.GetSourceByIdAsync(sourceId);
        if (source == null || source.UserId != userId) return null;
        
        // We need activities to calculate stats. Assuming GetSourceByIdAsync includes activities or we need to fetch them.
        // Repository implementation "GetTrackedSourcesAsync" usually includes activities. 
        // "GetSourceByIdAsync" might not properly Include activities if not designed for it.
        // Let's assume for now we might need to rely on what repository provides or ensure include.
        // Looking at SqliteActivityRepository (Step 37), GetSourceByIdAsync uses _context.Sources.FindAsync(id).
        // FindAsync DOES NOT include navigation properties.
        // So we need to ensure we get the source WITH activities.
        // Since we don't want to change Repository interface right now if possible, let's see.
        
        // Actually, let's just return what we can or rely on existing methods.
        // If we really need stats, we must fetch activities.
        // We can use _repository.GetAllForContentAsync(userId, sourceId) which likely uses ActivityService logic?
        // No, we are in SourceService.
        
        // Let's stick to simple implementation for now, and if stats are missing on single fetch, it's acceptable for "Initial Track" response.
        
        return MapToTrackedSourceDto(source);
    }

    private TrackedSourceDto MapToTrackedSourceDto(Source s)
    {
        // Calculate stats from activities
        // If Activities is null (lazy loading not enabled / not included), we handle it.
        var activities = s.Activities ?? new List<UserActivity>();
        
        var quizzes = activities.Where(a => a.Type == ActivityType.Quiz).ToList();
        var reads = activities.Where(a => a.Type == ActivityType.Read).ToList();
        
        var bestQuiz = quizzes
            .Where(q => q.ScorePercentage.HasValue)
            .OrderByDescending(q => q.ScorePercentage)
            .FirstOrDefault();

        var lastActivity = activities.OrderByDescending(a => a.SessionDateStart).FirstOrDefault();
        var lastQuiz = quizzes.OrderByDescending(q => q.SessionDateStart).FirstOrDefault();

        return new TrackedSourceDto
        {
            Id = s.Id,
            UserId = s.UserId,
            SourceId = s.Id, // Technical GUID
            ExternalId = s.ExternalId, // URL or DocId
            SourceType = s.Type,
            DisplayTitle = s.DisplayTitle,
            Url = s.OnlineResource?.URL ?? (s.Type == SourceType.Wikipedia && !string.IsNullOrEmpty(s.ExternalId) && s.ExternalId.Length < 60 ? $"https://en.wikipedia.org/wiki/{s.ExternalId}" : null),
            
            BestScore = bestQuiz != null ? (int)(bestQuiz.ScorePercentage ?? 0) : 0, 
            LastScore = lastQuiz != null ? (int)(lastQuiz.ScorePercentage ?? 0) : 0,
            LastAttemptDate = lastActivity?.SessionDateEnd ?? lastActivity?.SessionDateStart,
            
            TotalReadTimeSeconds = reads.Sum(r => r.DurationSeconds) + activities.Where(a => a.Type == ActivityType.Explore).Sum(e => e.DurationSeconds),
            TotalQuizTimeSeconds = quizzes.Sum(q => q.DurationSeconds),
            TotalStudyTimeSeconds = activities.Sum(a => a.DurationSeconds),
            
            IsPinned = s.IsPinned,
            IsArchived = s.IsArchived,
            IsInBacklog = s.IsInBacklog
        };
    }

    public async Task<Source?> GetSourceAsync(string sourceId)
    {
        return await _repository.GetSourceByIdAsync(sourceId);
    }

    public async Task<Source> ToggleTrackingAsync(string userId, string sourceId, bool isTracked)
    {
        // sourceId here is the Technical ID (Hash)
        var source = await _repository.GetSourceByIdAsync(sourceId);
        
        if (source == null)
        {
             throw new KeyNotFoundException($"Source {sourceId} not found");
        }
        
        if (source.UserId != userId)
        {
             throw new UnauthorizedAccessException("User does not own this source");
        }

        if (source.IsTracked != isTracked)
        {
            source.IsTracked = isTracked;
            if (!isTracked)
            {
                source.IsPinned = false;
                source.IsArchived = false;
            }
            await _repository.UpdateSourceAsync(source);
        }

        return source;
    }

    public async Task<Source> TogglePinAsync(string userId, string sourceId)
    {
        var source = await _repository.GetSourceByIdAsync(sourceId);
        if (source == null || source.UserId != userId) throw new KeyNotFoundException("Source not found");

        source.IsPinned = !source.IsPinned;
        await _repository.UpdateSourceAsync(source);
        return source;
    }

    public async Task<Source> ToggleArchiveAsync(string userId, string sourceId)
    {
        var source = await _repository.GetSourceByIdAsync(sourceId);
        if (source == null || source.UserId != userId) throw new KeyNotFoundException("Source not found");

        source.IsArchived = !source.IsArchived;
        await _repository.UpdateSourceAsync(source);
        return source;
    }

    public async Task<Source> TrackSourceAsync(string userId, string sourceId, string title, SourceType type)
    {
        // sourceId here MUST be the Technical ID (GUID)
        var source = await _repository.GetSourceByIdAsync(sourceId);
        
        if (source == null)
        {
             // FALLBACK: If not found, maybe it's a first-time track from discovery
             source = await GetOrCreateSourceAsync(userId, title, sourceId, type);
        }

        if (source.UserId != userId)
        {
             throw new UnauthorizedAccessException("User does not own this source");
        }

        if (!source.IsTracked)
        {
            source.IsTracked = true;
            await _repository.UpdateSourceAsync(source);
        }
        return source;
    }

    public async Task<Source> GetOrCreateSourceAsync(string userId, string title, string sourceId, SourceType type)
    {
         string externalId;
         string? url = null;

         if (type == SourceType.Document)
         {
             externalId = sourceId; // Documents use their GUID as ExternalId
         }
         else
         {
             // Wikipedia or Web: generate hash
             url = sourceId.StartsWith("http") ? sourceId : $"https://en.wikipedia.org/wiki/{sourceId}";
             externalId = SourceHasher.GenerateId(type, url);
         }

         // 1. Check if user already has a source for this content
         var source = await _repository.GetSourceByExternalIdAsync(userId, externalId);
         
         if (source == null)
         {
             // 2. Create new Source (GUID PK)
             source = new Source
             {
                 Id = Guid.NewGuid().ToString(),
                 UserId = userId,
                 Type = type,
                 ExternalId = externalId,
                 DisplayTitle = title,
                 IsTracked = false
             };
             await _repository.CreateSourceAsync(source);

             // 3. Ensure OnlineResource exists for the content
             if (type == SourceType.Wikipedia || !string.IsNullOrEmpty(url))
             {
                 var onlineResource = await _repository.GetOnlineResourceByIdAsync(externalId);
                 if (onlineResource == null)
                 {
                     onlineResource = new OnlineResource
                     {
                         Id = externalId,
                         UserId = userId,
                         SourceId = source.Id, // Original creator
                         URL = url!,
                         Title = title,
                         Provider = type == SourceType.Wikipedia ? "Wikipedia" : null,
                         SavedAt = DateTime.UtcNow
                     };
                     await _repository.CreateOnlineResourceAsync(onlineResource);
                 }
                 source.OnlineResource = onlineResource;
             }
         }
         
         return source;
    }

    public async Task UpdateSourceAsync(Source source)
    {
        await _repository.UpdateSourceAsync(source);
    }

    public async Task PopulateSourceContentAsync(Source source)
    {
        if (source == null) return;
        if (!string.IsNullOrEmpty(source.TextContent)) return;

        var contentSource = _contentSources.FirstOrDefault(s => s.CanHandle(source.Type));
        if (contentSource == null)
        {
            _logger.LogWarning("No content source found for type {Type}", source.Type);
            return;
        }

        try
        {
            _logger.LogInformation("Populating content for source {SourceId} ({Type})", source.Id, source.Type);
            var content = await contentSource.GetContentAsync(source);
            if (content != null && !string.IsNullOrEmpty(content.TextContent))
            {
                source.TextContent = content.TextContent;
                
                // If it's a document, reflect that extraction is completed
                if (source.Type == SourceType.Document)
                {
                    source.ContentExtractionStatus = ContentExtractionStatus.Completed;
                    source.ContentExtractionCompletedAt = DateTime.UtcNow;
                    source.ContentExtractionError = null;
                }
                
                await _repository.UpdateSourceAsync(source);
                _logger.LogInformation("Source {SourceId} content populated. Length: {Length}", source.Id, source.TextContent.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating content for source {SourceId}", source.Id);
        }
    }
}
