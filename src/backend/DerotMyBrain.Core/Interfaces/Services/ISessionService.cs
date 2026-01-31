using DerotMyBrain.Core.DTOs;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface ISessionService
{
    Task<UserSessionDto?> GetSessionByIdAsync(string userId, string sessionId);
    Task<IEnumerable<UserSessionDto>> GetSessionsByUserIdAsync(string userId);
    Task StopSessionAsync(string userId, string sessionId);
}
