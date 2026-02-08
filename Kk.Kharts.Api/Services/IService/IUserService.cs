using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IUserService
    {
        Task<(bool Success, string ErrorMessage, UserDTO User)> CreateUserAsync(UserCreateDTO dto);
    }

}
