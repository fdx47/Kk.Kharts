using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class SoilParameterRepository : ISoilParameterRepository
    {
        private readonly AppDbContext _dbContext;

        public SoilParameterRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IReadOnlyList<SoilParameter>> GetAllSoilParameterRepositoryAsync()
        {
            return await _dbContext.SoilParameters
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<SoilParameter?> GetSoilParameterByIdAsync(int id)
        {
            return await _dbContext.SoilParameters
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

    }
}

