using System.IO;
using System.Threading.Tasks;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the storage system.
    /// </summary>
    /// <param name="fileStream">The input stream of the file.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="subDirectory">Optional sub-directory to organize files (e.g. by User ID).</param>
    /// <returns>The relative path or identifier of the stored file.</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subDirectory = "");

    /// <summary>
    /// Deletes a file from the storage system.
    /// </summary>
    /// <param name="filePath">The relative path or identifier of the file.</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Retrieves a file stream.
    /// </summary>
    /// <param name="filePath">The relative path or identifier of the file.</param>
    /// <returns>Stream of the file content.</returns>
    Task<Stream> GetFileStreamAsync(string filePath);

    /// <summary>
    /// Gets the absolute path for a stored file (for display purposes).
    /// </summary>
    string GetAbsolutePath(string relativePath);
}
