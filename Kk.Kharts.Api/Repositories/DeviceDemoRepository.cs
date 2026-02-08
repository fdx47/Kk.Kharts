using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Repositories.IRepository;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Repositories
{
    public class DeviceDemoRepository : IDeviceDemoRepository
    {
        private readonly AppDbContext _context;

        public DeviceDemoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DeviceDemo>> GetAllAsync()
        {
            return await _context.DevicesDemos.ToListAsync();
        }

        public async Task<DeviceDemo?> GetByDevEuiAsync(string devEui)
        {
            return await _context.DevicesDemos.FindAsync(devEui);
        }

        public async Task<DeviceDemo> CreateAsync(DeviceDemo device)
        {
            _context.DevicesDemos.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<bool> UpdateAsync(DeviceDemo device)
        {
            var existingDevice = await _context.DevicesDemos.FindAsync(device.DevEui);
            if (existingDevice == null)
                return false;

            existingDevice.DevEui = device.DevEui;
            existingDevice.Name = device.Name;
            existingDevice.Description = device.Description;
            existingDevice.InstallationLocation = device.InstallationLocation;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string devEui)
        {
            var device = await _context.DevicesDemos.FindAsync(devEui);
            if (device == null)
                return false;

            _context.DevicesDemos.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
