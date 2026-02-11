using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IUserService
    {
        Task<(bool Success, string ErrorMessage, UserDTO User)> CreateUserAsync(UserCreateDTO dto);

        /// <summary>
        /// Recherche un utilisateur par son adresse e-mail (lecture seule).
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Recherche un utilisateur par son refresh token actif.
        /// </summary>
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Met à jour les champs d'authentification d'un utilisateur
        /// (RefreshToken, RefreshTokenExpiryTime, LastIpAddress, LastUserAgent).
        /// </summary>
        Task UpdateUserAuthDataAsync(User user);
    }

}
