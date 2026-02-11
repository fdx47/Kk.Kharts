using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Threading;

namespace Kk.Kharts.Api.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies
                .Include(c => c.ParentCompany)
                .Include(c => c.Subsidiaries)
                .ToListAsync();
        }

        public async Task<Company?> GetByIdAsync(int companyId, AuthenticatedUserDto authenticatedUser)
        {
            if (authenticatedUser == null)
                throw new ArgumentNullException(nameof(authenticatedUser));


            if (Roles.NoRoot.Contains(authenticatedUser.Role))
            {
                // Usuários normais só vêem se forem parte da company
                return await _context.Companies
                    .Include(c => c.ParentCompany)
                    .Include(c => c.Subsidiaries)
                    .Where(c => c.Id == companyId && c.Users.Any(u => u.Id == authenticatedUser.UserId))
                    .FirstOrDefaultAsync();

            }
            else if (authenticatedUser.Role == Roles.Root)
            {
                // Root vê qualquer company sem restrição
               var fdx = await _context.Companies
                    .Include(c => c.ParentCompany)
                    .Include(c => c.Subsidiaries)
                    .FirstOrDefaultAsync(c => c.Id == companyId);
                return fdx;
            }

            return new Company();

        }

        public async Task AddAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Company company)
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Busca uma company por ID sem verificação de autenticação.
        /// Uso interno para verificação de hierarquia de companies.
        /// </summary>
        public async Task<Company?> GetByIdInternalAsync(int companyId)
        {
            return await _context.Companies
                .Include(c => c.ParentCompany)
                .Include(c => c.Subsidiaries)
                .FirstOrDefaultAsync(c => c.Id == companyId);
        }

        public async Task<Company?> GetByApiKeyAsync(string headerName, string headerValue, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue))
            {
                return null;
            }

            var sanitizedHeaderName = headerName.Trim();
            var sanitizedHeaderValue = headerValue.Trim();

            return await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    company => company.HeaderNameApiKey != null &&
                               company.HeaderValueApiKey != null &&
                               company.HeaderNameApiKey.ToUpper() == sanitizedHeaderName.ToUpper() &&
                               company.HeaderValueApiKey == sanitizedHeaderValue,
                    cancellationToken);
        }
    }
}
