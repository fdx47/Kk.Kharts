using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService
{
    public interface ITwoFactorService
    {
        /// <summary>
        /// Génère un nouveau secret TOTP pour l'utilisateur
        /// </summary>
        TwoFactorSetupResponseDTO GenerateSetupInfo(User user);

        /// <summary>
        /// Vérifie si le code TOTP fourni est valide
        /// </summary>
        bool ValidateCode(string secret, string code);

        /// <summary>
        /// Active le 2FA pour l'utilisateur après vérification du code
        /// </summary>
        Task<bool> EnableTwoFactorAsync(int userId, string code);

        /// <summary>
        /// Désactive le 2FA pour l'utilisateur
        /// </summary>
        Task<bool> DisableTwoFactorAsync(int userId, string code);

        /// <summary>
        /// Définit si le 2FA est obligatoire pour un utilisateur (admin only)
        /// </summary>
        Task<bool> SetTwoFactorRequiredAsync(int userId, bool required);

        /// <summary>
        /// Récupère le statut 2FA d'un utilisateur
        /// </summary>
        Task<TwoFactorStatusDTO> GetTwoFactorStatusAsync(int userId);

        /// <summary>
        /// Génère l'URI pour le QR code compatible Google/Microsoft Authenticator
        /// </summary>
        string GenerateQrCodeUri(string secret, string email, string issuer = "KropKontrol");
    }
}
