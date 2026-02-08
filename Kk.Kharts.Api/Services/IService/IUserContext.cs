using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IUserContext
    {
        Task<AuthenticatedUserDto> GetUserInfoFromToken(); 
    }
}
