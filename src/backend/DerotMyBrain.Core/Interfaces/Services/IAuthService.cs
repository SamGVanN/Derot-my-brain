using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Core.Interfaces.Services;

public interface IAuthService
{
    string GenerateIdentityToken(User user);
}
