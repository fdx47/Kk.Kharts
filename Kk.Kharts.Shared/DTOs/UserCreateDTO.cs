using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.DTOs
{
    public class UserCreateDTO
    {
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        public string HeaderNameApiKey { get; set; } = "?3ctec??3CtEc?";
        public string HeaderValueApiKey { get; set; } = "?3¤*µTt$c??3cec47?";
    }
}


