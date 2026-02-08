using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface IDeviceDemoRepository
    {
        Task<IEnumerable<DeviceDemo>> GetAllAsync();
        Task<DeviceDemo?> GetByDevEuiAsync(string devEui);
        Task<DeviceDemo> CreateAsync(DeviceDemo device);
        Task<bool> UpdateAsync(DeviceDemo device);
        Task<bool> DeleteAsync(string devEui);
    }
}
