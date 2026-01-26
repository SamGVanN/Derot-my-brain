using System;
using System.IO;
using System.Threading.Tasks;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DerotMyBrain.Infrastructure.Services;

public class FileSystemStorageService : IFileStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<FileSystemStorageService> _logger;

    public FileSystemStorageService(IConfiguration configuration, ILogger<FileSystemStorageService> logger)
    {
        _logger = logger;
        
        // 1. Check configuration for override
        var configPath = configuration["FileStorage:RootPath"];
        
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            _storageRoot = configPath;
        }
        else
        {
            // 2. Default to User AppData to avoid permission issues
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _storageRoot = Path.Combine(appData, "DerotMyBrain", "Uploads");
        }

        if (!Directory.Exists(_storageRoot))
        {
            try
            {
                Directory.CreateDirectory(_storageRoot);
                _logger.LogInformation("Created storage root at: {StorageRoot}", _storageRoot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create storage root at: {StorageRoot}", _storageRoot);
                // Fallback to local data folder if AppData fails? 
                // For now, let it throw so we know there's a serious env issue.
                throw;
            }
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subDirectory = "")
    {
        var safeFileName = $"{Guid.NewGuid()}_{fileName}";
        var targetDir = string.IsNullOrWhiteSpace(subDirectory) 
            ? _storageRoot 
            : Path.Combine(_storageRoot, subDirectory);

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var fullPath = Path.Combine(targetDir, safeFileName);
        
        using (var destStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destStream);
        }
        
        // Return relative path for portability
        return string.IsNullOrWhiteSpace(subDirectory) 
            ? safeFileName 
            : Path.Combine(subDirectory, safeFileName);
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_storageRoot, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file: {FilePath}", fullPath);
        }
        else
        {
            _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> GetFileStreamAsync(string filePath)
    {
        var fullPath = Path.Combine(_storageRoot, filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public string GetAbsolutePath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(_storageRoot, relativePath));
    }
}
