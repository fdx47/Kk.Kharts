using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class Wet150Sdi12MetadataRepository : IWet150Sdi12MetadataRepository
    {
        private readonly AppDbContext _context;

        public Wet150Sdi12MetadataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Wet150Sdi12Metadata>> GetByDevEuiAsync(string devEui)
        {
            return await _context.Wet150Sdi12MultiSensorMetadatas
                .AsNoTracking()
                .Where(x => x.DevEui == devEui)
                .ToListAsync();
        }

        public async Task<Wet150Sdi12Metadata?> GetByDevEuiAndIndexAsync(string devEui, int index)
        {
            return await _context.Wet150Sdi12MultiSensorMetadatas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DevEui == devEui && x.Sdi12Index == index);
        }


        public async Task AddAsync(Wet150Sdi12Metadata entity)
        {
            _context.Wet150Sdi12MultiSensorMetadatas.Add(entity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Wet150Sdi12Metadata>> GetAllAsync()
        {
            return await _context.Wet150Sdi12MultiSensorMetadatas.AsNoTracking().ToListAsync();
        }


        public async Task<Wet150Sdi12Metadata?> GetByIdAsync(int id)
        {
            return await _context.Wet150Sdi12MultiSensorMetadatas.FindAsync(id);
        }


        public async Task UpdateAsync(Wet150Sdi12Metadata entity)
        {
            _context.Wet150Sdi12MultiSensorMetadatas.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Wet150Sdi12MultiSensorMetadatas.FindAsync(id);
            if (entity != null)
            {
                _context.Wet150Sdi12MultiSensorMetadatas.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }


    }
}
