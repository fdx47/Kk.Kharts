using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class Em300DiRepository : IEm300DiRepository
    {
        private readonly AppDbContext _context;

        public Em300DiRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<Em300Di>> GetEm300DataByDevEuiAsync(string devEui, DateTime startDate, DateTime endDate)
        {
            return await _context.Em300Dis
                .AsNoTracking()
                .Where(x => x.DevEui == devEui && x.Timestamp >= startDate && x.Timestamp <= endDate)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();
        }


        async Task<Em300Di> IEm300DiRepository.AddEntityAndSaveAsync(Em300Di entity, string devEui)
        {
            await _context.Em300Dis.AddAsync(entity);  // Adicionar a entidade           
            await _context.SaveChangesAsync();       // Salvar as alterações

            return entity;
        }


    }
}
