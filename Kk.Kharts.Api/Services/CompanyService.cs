using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Companies;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IHashIdService _hashIdService;

        public CompanyService(ICompanyRepository repo, IHashIdService hashIdService)
        {
            _companyRepository = repo;
            _hashIdService = hashIdService;
        }


        public async Task<List<CompanyDto>> GetAllCompaniesAsyncOLD()
        {
            var companies = await _companyRepository.GetAllAsync();
            //return companies.Select(c => new CompanyDto
            //{
            //    Id = c.Id,
            //    Name = c.Name!,
            //    ParentCompanyId = c.ParentCompanyId,
            //    ParentCompanyName = c.ParentCompany?.Name,
            //    SubsidiariesCount = c.Subsidiaries?.Count ?? 0
            //}).ToList();


            return [.. companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name!,
                ParentCompanyId = c.ParentCompanyId,
                ParentCompanyName = c.ParentCompany?.Name,
                SubsidiariesCount = c.Subsidiaries?.Count ?? 0
            })];

        }


        public async Task<List<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return companies.Select(selector: MapToDto).ToList();
        }


        public async Task<CompanyDto?> GetCompanyByIdAsync(int companyId, AuthenticatedUserDto? authenticatedUser = null)
        {
            var userContext = authenticatedUser ?? new AuthenticatedUserDto { Role = "Root" };
            var company = await _companyRepository.GetByIdAsync(companyId, userContext);
            return company == null ? null : MapToDto(company);
        }


        public async Task<CompanyDto> CreateCompanyAsync(CompanyCreateDTO dto)
        {
            var newCompany = new Company
            {
                Name = dto.Name,
                ParentCompanyId = dto.ParentCompanyId
            };

            await _companyRepository.AddAsync(newCompany);
            return MapToDto(newCompany);
        }


        public async Task<CompanyDto?> UpdateCompanyAsync(int id, CompanyUpdateDTO dto)
        {
            var company = await _companyRepository.GetByIdAsync(id, null!);
            if (company == null) return null;

            company.Name = dto.Name ?? string.Empty;
            company.ParentCompanyId = dto.ParentCompanyId;

            await _companyRepository.UpdateAsync(company);
            return MapToDto(company);
        }

        //public async Task<bool> DeleteCompanyAsync(int id)
        //{
        //    var company = await _companyRepository.GetByIdAsync(id);
        //    if (company == null) return false;

        //    await _companyRepository.DeleteAsync(company);
        //    return true;
        //}

        //public async Task<bool> DisableCompanyAsync(int id)
        //{
        //    var company = await _companyRepository.GetByIdAsync(id);
        //    if (company == null)
        //        return false;

        //    company.IsActive = false;
        //    await _companyRepository.SaveChangesAsync();

        //    return true;
        //}



        public async Task<bool> DisableCompanyAsync(int id)
        {
            var defaultUser = new AuthenticatedUserDto { Role = "Root" };
            var company = await _companyRepository.GetByIdAsync(id, defaultUser);
            if (company == null)
                //throw new NotFoundExceptionKk("Entreprise non trouvée.");
                return false;

            company.IsActive = false;
            await _companyRepository.SaveChangesAsync();
            return true;
        }


        private CompanyDto MapToDto(Company c)
        {
            return new CompanyDto
            {
                Id = c.Id,
                PublicId = _hashIdService.Encode(c.Id) ?? string.Empty,
                Name = c.Name ?? string.Empty,
                ParentCompanyId = c.ParentCompanyId,
                ParentCompanyName = c.ParentCompany?.Name,
                SubsidiariesCount = c.Subsidiaries?.Count ?? 0,
                IsActive = c.IsActive
            };
        }


    }
}


