using Kk.Kharts.Shared.Enums;

namespace Kk.Kharts.Shared.DTOs
{
    public class AuthenticatedUserDto
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public UserAccessLevel AccessLevel { get; set; }
        public int CompanyId { get; set; }

        public bool IsCompanyActive { get; set; }

        // Liste des devices accessibles pour le technician
        public List<int>? AccessibleDeviceIds { get; set; }
    }
}
