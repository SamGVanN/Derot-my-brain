// Trigger reload
namespace DerotMyBrain.API.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class UserList
    {
        public List<User> Users { get; set; } = [];
    }
}
