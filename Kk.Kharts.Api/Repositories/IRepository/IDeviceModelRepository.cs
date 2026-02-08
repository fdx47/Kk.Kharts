using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface IDeviceModelRepository
    {
        Task<List<DeviceModel>> GetAllRepositoryAsync();
        Task<DeviceModel?> GetModelByDevEuiAsync(string devEui);

    }
}
