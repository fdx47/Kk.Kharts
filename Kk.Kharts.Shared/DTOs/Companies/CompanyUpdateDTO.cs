namespace Kk.Kharts.Shared.DTOs.Companies
{
    public class CompanyUpdateDTO
    {
        public string Name { get; set; } = string.Empty;
        public int? ParentCompanyId { get; set; }
    }
}
