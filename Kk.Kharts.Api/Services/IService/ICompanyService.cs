

using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Companies;

namespace Kk.Kharts.Api.Services.IService
{
    public interface ICompanyService
    {
        Task<List<CompanyDto>> GetAllCompaniesAsync();
        Task<CompanyDto?> GetCompanyByIdAsync(int companyId, AuthenticatedUserDto? authenticatedUser = null);
        Task<CompanyDto> CreateCompanyAsync(CompanyCreateDTO dto);
        Task<CompanyDto?> UpdateCompanyAsync(int id, CompanyUpdateDTO dto);
        Task<bool> DisableCompanyAsync(int id);
    }
}
