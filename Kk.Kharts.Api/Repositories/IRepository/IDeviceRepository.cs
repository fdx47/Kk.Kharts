using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

public interface IDeviceRepository
{

    Task<List<Device>> GetAllDevicesRepositoryAsync(AuthenticatedUserDto authenticatedUser);

    Task<Device?> GetDeviceByDevEuiRepositoryAsync(string devEui, AuthenticatedUserDto authenticatedUser);
    Task<Device?> GetDeviceByDevEuiApiKeyAsync(string devEui);

    //Task<Device?> GetDeviceByIdApiKeyAsync(int? deviceId);
    Task<Device?> GetDeviceByIdApiKeyAsync(string devEui);
    Task UpdateDeviceAsync(Device device);
    Task UpdateDeviceStatusAsync(string devEui, float batteryValue, string lastSendAt, DateTime lastSeenAt, TimeSpan lastSeenAtDuration);

    Task UpdateAsync(Device device, AuthenticatedUserDto authenticatedUser);
    Task SaveChangesAsync();

    Task AddAsync(Device device);
    Task<bool> ExistsAsync(string devEui);



    Task<Device> GetByDevEuiAsync(string devEui);
    Task<Device> GetByIdAsync(int id);


}
