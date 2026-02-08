using System.ComponentModel.DataAnnotations.Schema;

namespace Kk.Kharts.Shared.Entities
{

    public class Company
    {
        public int Id { get; set; }
        public Guid DeviceId { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public int? ParentCompanyId { get; set; }

        public bool IsActive { get; set; } = true;

        public string HeaderNameApiKey { get; set; } = string.Empty;
        public string HeaderValueApiKey { get; set; } = string.Empty;
        public Company? ParentCompany { get; set; }
        public ICollection<Company>? Subsidiaries { get; set; }

        // Relação um-para-muitos com User
        public ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Public obfuscated ID for external exposure.
        /// Not mapped to database - computed from Id.
        /// </summary>
        [NotMapped]
        public string? PublicId { get; set; }
    }


}
