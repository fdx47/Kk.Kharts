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

        /// <summary>
        /// Récupère tous les utilisateurs sous forme de DTOs.
        /// </summary>
        Task<List<UserDTO>> GetAllUsersAsync();

        /// <summary>
        /// Récupère un utilisateur par son ID.
        /// </summary>
        Task<User?> GetUserByIdAsync(int id);

        /// <summary>
        /// Met à jour un utilisateur existant (admin Root uniquement).
        /// </summary>
        Task<bool> UpdateUserAsync(int id, UserAdminUpdateDTO dto);

        /// <summary>
        /// Supprime un utilisateur par son ID.
        /// </summary>
        Task DeleteUserAsync(int id);

        /// <summary>
        /// Met à jour l'email et/ou le mot de passe d'un utilisateur (self-service).
        /// </summary>
        Task<bool> UpdateSelfAccountAsync(int userId, UserUpdateSelfDTO dto);

        /// <summary>
        /// Vérifie si un e-mail est déjà utilisé.
        /// </summary>
        Task<bool> IsEmailInUseAsync(string email);

        /// <summary>
        /// Crée une demande de changement d'e-mail et retourne le token.
        /// </summary>
        Task<string> CreateEmailChangeRequestAsync(int userId, string newEmail);

        /// <summary>
        /// Confirme un changement d'e-mail via token.
        /// </summary>
        Task<bool> ConfirmEmailChangeAsync(string token);

        /// <summary>
        /// Crée une demande de réinitialisation de mot de passe et retourne le token (null si l'utilisateur n'existe pas).
        /// </summary>
        Task<string?> CreatePasswordResetRequestAsync(string email);

        /// <summary>
        /// Confirme la réinitialisation du mot de passe via token.
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> ConfirmPasswordResetAsync(string token, string newPassword);
    }
}
