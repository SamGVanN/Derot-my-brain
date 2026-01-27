using System.Collections.Generic;
using System.Threading.Tasks;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IBacklogService
{
    /// <summary>
    /// Adds a source (Wikipedia article or Document) to the backlog.
    /// Idempotent: checks for existing items by SourceHash.
    /// </summary>
    Task<BacklogItem> AddToBacklogAsync(string userId, string sourceId, SourceType sourceType, string title, string? url = null, string? provider = null);

    /// <summary>
    /// Removes an item from the backlog.
    /// </summary>
    Task RemoveFromBacklogAsync(string userId, string sourceId);

    /// <summary>
    /// Gets the user's backlog.
    /// </summary>
    Task<IEnumerable<BacklogItem>> GetUserBacklogAsync(string userId);
    
    /// <summary>
    /// Checks if a source is currently in the backlog.
    /// </summary>
    Task<bool> IsInBacklogAsync(string userId, string sourceId);
}
