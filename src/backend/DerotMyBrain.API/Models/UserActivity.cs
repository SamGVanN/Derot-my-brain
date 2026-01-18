
namespace DerotMyBrain.API.Models
{
    public class UserActivity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // e.g., "Read", "Quiz"
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? Score { get; set; }
    }
}
