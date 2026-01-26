public interface ILlmClient
{
    Task<string> GenerateAsync(string prompt);
}
