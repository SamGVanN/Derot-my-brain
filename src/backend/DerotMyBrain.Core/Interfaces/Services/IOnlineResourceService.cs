using System.Threading.Tasks;

namespace DerotMyBrain.Core.Interfaces.Services;

/// <summary>
/// Service responsbile for managing OnlineResource logic (URL validation, metadata extraction, etc.).
/// NOTE: Initially, implementation details might be delegated to SourceService or BacklogService to avoid over-engineering.
/// This interface exists to enforce architectural boundaries and allow future expansion (e.g. specific parsers for Youtube/Medium).
/// </summary>
public interface IOnlineResourceService
{
    // Placeholder for future logic
    // Task<OnlineResourceMetadata> ExtractMetadataAsync(string url);
    // Task<bool> ValidateUrlAsync(string url);
}
