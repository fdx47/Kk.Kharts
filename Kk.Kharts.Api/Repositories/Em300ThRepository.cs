using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities.Em300;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class Em300ThRepository : IEm300ThRepository
    {
        private readonly AppDbContext _context;

        public Em300ThRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<Em300Th>> GetEm300DataByDevEuiAsync(string devEui, DateTime startDate, DateTime endDate)
        {
            return await _context.Em300ths
                .AsNoTracking()
                .Where(x => x.DevEui == devEui && x.Timestamp >= startDate && x.Timestamp <= endDate)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();
        }


        async Task<Em300Th> IEm300ThRepository.AddEntityAndSaveAsync(Em300Th entity, string devEui)
        {
            await _context.Em300ths.AddAsync(entity);  // Adicionar a entidade           
            await _context.SaveChangesAsync();       // Salvar as alterações

            return entity;
        }


    }
}
