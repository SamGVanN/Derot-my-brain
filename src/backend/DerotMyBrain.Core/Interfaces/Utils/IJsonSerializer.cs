namespace DerotMyBrain.Core.Interfaces.Utils;

public interface IJsonSerializer
{
    string Serialize<T>(T value);
    T? Deserialize<T>(string json);
}
