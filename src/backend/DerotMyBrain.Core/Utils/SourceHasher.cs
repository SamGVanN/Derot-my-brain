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
    /// Generates a SHA-256 hash from its source type and id.
    /// This is used to create a fixed-length identifier for potentially long URLs or file paths.
    /// </summary>
    /// <param name="sourceType">The origin of the content.</param>
    /// <param name="sourceId">The raw identifier (e.g., URL, File Path).</param>
    /// <returns>A 64-character hexadecimal string representing the SHA-256 hash.</returns>
    public static string GenerateHash(SourceType sourceType, string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("SourceId cannot be empty", nameof(sourceId));

        // Use enum name for consistent hashing
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
