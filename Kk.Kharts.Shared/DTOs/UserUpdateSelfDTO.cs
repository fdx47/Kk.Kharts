using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.DTOs
{
    public class UserUpdateSelfDTO
    {
        [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        public string Password { get; set; } = null!;
    }

}
