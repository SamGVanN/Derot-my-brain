using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.DTOs;

public class UserSessionDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public SourceDto Source { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public SessionStatus Status { get; set; }
    public List<UserActivityDto> Activities { get; set; } = new();
}
