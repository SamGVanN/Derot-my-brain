using System.Text.Json;
using DerotMyBrain.Core.Interfaces.Utils;

namespace DerotMyBrain.Infrastructure.Utils;

public class JsonSerializerWrapper : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
