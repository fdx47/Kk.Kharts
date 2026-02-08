using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAllAsync();
        Task<Company?> GetByIdAsync(int companyId, AuthenticatedUserDto authenticatedUser);
        /// <summary>
        /// Busca uma company por ID sem verificação de autenticação. Uso interno.
        /// </summary>
        Task<Company?> GetByIdInternalAsync(int companyId);
        Task<Company?> GetByApiKeyAsync(string headerName, string headerValue, CancellationToken cancellationToken = default);
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Company company);
        Task SaveChangesAsync();
    }
}
