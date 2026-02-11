using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService;

public interface IJwtService
{
    string GenerateJwtToken(User utilisateur, long? telegramId = null);
    string GenerateRefreshToken();
}
