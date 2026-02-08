using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class DeviceModelRepository : IDeviceModelRepository
    {
        private readonly AppDbContext _context;

        public DeviceModelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeviceModel>> GetAllRepositoryAsync()
        {
            return await _context.Set<DeviceModel>()
                                 .AsNoTracking()
                                 .ToListAsync();
        }


        public async Task<DeviceModel?> GetModelByDevEuiAsync(string devEui)
        {
            return await _context.Devices
                .Include(d => d.ModeloNavegacao)
                .Where(d => d.DevEui == devEui)
                .Select(d => d.ModeloNavegacao)
                .FirstOrDefaultAsync();
        }


    }

}
