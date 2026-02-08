namespace Kk.Kharts.Shared.DTOs.Companies
{
    public class CompanyCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public int? ParentCompanyId { get; set; }
    }
}
