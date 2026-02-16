namespace Kk.Kharts.Shared.DTOs.Companies
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? ParentCompanyId { get; set; }
        public string? ParentCompanyName { get; set; }
        public int SubsidiariesCount { get; set; }
        public bool IsActive { get; set; }
        public string HeaderNameApiKey { get; set; } = string.Empty;
        public string HeaderValueApiKey { get; set; } = string.Empty;
    }
}
