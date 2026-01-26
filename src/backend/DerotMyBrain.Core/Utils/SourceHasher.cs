using System.Security.Cryptography;
using System.Text;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Utils;

/// <summary>
/// Provides centralized logic for generating deterministic hashes for content identification.
/// This ensures consistent SourceHash generation across the application.
/// </summary>
public static class SourceHasher
{
    /// <summary>
    /// Generates a technical ID for a Source.
    /// Documents return their original GUID ID.
    /// Wikipedia and others return a deterministic SHA-256 hash.
    /// </summary>
    /// <param name="sourceType">The origin of the content.</param>
    /// <param name="sourceId">The raw identifier (e.g., Page Title, GUID).</param>
    /// <returns>A unique identifier string.</returns>
    public static string GenerateId(SourceType sourceType, string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("SourceId cannot be empty", nameof(sourceId));

        // For Documents, the sourceId IS the Guid we want to use as the Source's PK
        if (sourceType == SourceType.Document)
        {
            return sourceId.Trim();
        }

        // For Wikipedia and others, we generate a deterministic hash
        var typeKey = sourceType.ToString().ToLowerInvariant();
        var idKey = sourceId.Trim().ToLowerInvariant();
        
        var input = $"{typeKey}:{idKey}";
        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        
        var builder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }
        
        return builder.ToString();
    }
}
