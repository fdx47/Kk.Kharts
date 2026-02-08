using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Enums;
using System.Security.Claims;

namespace Kk.Kharts.Api.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyService _companyService;

        public UserContext(IHttpContextAccessor httpContextAccessor, ICompanyService companyService)
        {
            _httpContextAccessor = httpContextAccessor;
            _companyService = companyService;
        }

        public async Task<AuthenticatedUserDto> GetUserInfoFromToken()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                throw new Exception("-<UserContext - GetUserInfoFromToken>- Contexte utilisateur introuvable");

            if (!int.TryParse(user.FindFirstValue(ClaimTypesCustom.SocieteId), out var companyId))
                throw new Exception("ID de l'entreprise invalide dans le token.");


            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? throw new InvalidOperationException("User ID claim not found in token.");
            var roleClaim = user.FindFirstValue(ClaimTypes.Role);
            var companyIdClaim = user.FindFirstValue(ClaimTypesCustom.SocieteId)
                ?? throw new InvalidOperationException("Company ID claim not found in token.");
            var accessLevelClaim = user.FindFirstValue(ClaimTypesCustom.NiveauAcces)
                ?? throw new InvalidOperationException("Access level claim not found in token.");

            var authenticatedUser = new AuthenticatedUserDto
            {
                UserId = int.Parse(userIdClaim),
                Role = roleClaim ?? string.Empty,
                CompanyId = int.Parse(companyIdClaim),
                AccessLevel = (UserAccessLevel)int.Parse(accessLevelClaim),
            };


            var company = await _companyService.GetCompanyByIdAsync(companyId, authenticatedUser);

            if (company == null || !company.IsActive)
                throw new UnauthorizedAccessException("Entreprise inactivée ou non trouvée.");

            AuthenticatedUserDto authResponse = new AuthenticatedUserDto
            {
                UserId = authenticatedUser.UserId,
                Role = authenticatedUser.Role ?? Roles.User,
                CompanyId = authenticatedUser.CompanyId,
                AccessLevel = authenticatedUser.AccessLevel,
                IsCompanyActive = company?.IsActive ?? false,
                AccessibleDeviceIds = null 
            };

            return authResponse;
        }

    }
}
