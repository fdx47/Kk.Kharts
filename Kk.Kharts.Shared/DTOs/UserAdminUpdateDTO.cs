using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.DTOs
{
    /// <summary>
    /// DTO pour la mise à jour d'un utilisateur par un administrateur Root.
    /// Seuls les champs autorisés sont exposés — les champs sensibles
    /// (mot de passe, token, IP, etc.) ne sont pas modifiables via cet endpoint.
    /// </summary>
    public class UserAdminUpdateDTO
    {
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        public string? HeaderName { get; set; }
        public string? HeaderValue { get; set; }
    }
}
