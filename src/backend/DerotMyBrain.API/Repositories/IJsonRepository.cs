namespace DerotMyBrain.API.Repositories
{
    public interface IJsonRepository<T>
    {
        Task<T> GetAsync(string fileName);
        Task SaveAsync(string fileName, T data);
        Task DeleteAsync(string fileName);
    }
}
