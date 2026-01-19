using System.Text.Json;

namespace DerotMyBrain.API.Repositories
{
    public class JsonRepository<T> : IJsonRepository<T> where T : new()
    {
        private readonly string _dataDirectory;

        public JsonRepository(IConfiguration configuration)
        {
            _dataDirectory = configuration["DataDirectory"] ?? "Data";
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public async Task<T> GetAsync(string fileName)
        {
            var filePath = Path.Combine(_dataDirectory, fileName);
            if (!File.Exists(filePath))
            {
                return new T();
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new T();
        }

        public async Task SaveAsync(string fileName, T data)
        {
            var filePath = Path.Combine(_dataDirectory, fileName);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public Task DeleteAsync(string fileName)
        {
            var filePath = Path.Combine(_dataDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }
    }
}
