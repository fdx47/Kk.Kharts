using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Companies;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IHashIdService _hashIdService;
        private readonly IUserContext _userContext;

        public CompanyService(ICompanyRepository repo, IHashIdService hashIdService, IUserContext userContext)
        {
            _companyRepository = repo;
            _hashIdService = hashIdService;
            _userContext = userContext;
        }


        public async Task<List<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return companies.Select(company => MapToDto(company, includeApiKeys: true)).ToList();
        }


        public async Task<CompanyDto?> GetCompanyByIdAsync(int companyId, AuthenticatedUserDto? authenticatedUser = null)
        {
            var userContext = authenticatedUser ?? new AuthenticatedUserDto { Role = Roles.Root };
            var company = await _companyRepository.GetByIdAsync(companyId, userContext);
            var canViewApiKeys = string.Equals(userContext.Role, Roles.Root, StringComparison.OrdinalIgnoreCase);
            return company == null ? null : MapToDto(company, includeApiKeys: canViewApiKeys);
        }


        public async Task<CompanyDto> CreateCompanyAsync(CompanyCreateDTO dto)
        {
            var newCompany = new Company
            {
                Name = dto.Name,
                ParentCompanyId = dto.ParentCompanyId,
                HeaderNameApiKey = dto.HeaderNameApiKey?.Trim() ?? string.Empty,
                HeaderValueApiKey = dto.HeaderValueApiKey?.Trim() ?? string.Empty
            };

            await _companyRepository.AddAsync(newCompany);
            return MapToDto(newCompany, includeApiKeys: true);
        }


        public async Task<CompanyDto?> UpdateCompanyAsync(int id, CompanyUpdateDTO dto)
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();
            var company = await _companyRepository.GetByIdAsync(id, authenticatedUser);
            if (company == null) return null;

            company.Name = dto.Name ?? string.Empty;
            company.ParentCompanyId = dto.ParentCompanyId;
            company.HeaderNameApiKey = dto.HeaderNameApiKey?.Trim() ?? company.HeaderNameApiKey;
            company.HeaderValueApiKey = dto.HeaderValueApiKey?.Trim() ?? company.HeaderValueApiKey;

            await _companyRepository.UpdateAsync(company);
            return MapToDto(company, includeApiKeys: true);
        }

        public async Task<bool> DisableCompanyAsync(int id)
        {
            var defaultUser = new AuthenticatedUserDto { Role = "Root" };
            var company = await _companyRepository.GetByIdAsync(id, defaultUser);
            if (company == null)
                return false;

            company.IsActive = false;
            await _companyRepository.SaveChangesAsync();
            return true;
        }


        private CompanyDto MapToDto(Company c, bool includeApiKeys = false)
        {
            return new CompanyDto
            {
                Id = c.Id,
                PublicId = _hashIdService.Encode(c.Id) ?? string.Empty,
                Name = c.Name ?? string.Empty,
                ParentCompanyId = c.ParentCompanyId,
                ParentCompanyName = c.ParentCompany?.Name,
                SubsidiariesCount = c.Subsidiaries?.Count ?? 0,
                IsActive = c.IsActive,
                HeaderNameApiKey = includeApiKeys ? c.HeaderNameApiKey : string.Empty,
                HeaderValueApiKey = includeApiKeys ? c.HeaderValueApiKey : string.Empty
            };
        }


    }
}


