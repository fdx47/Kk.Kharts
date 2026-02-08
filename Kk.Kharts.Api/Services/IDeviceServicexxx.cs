using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IDeviceServicexxx
    {
        Task<T?> GetDeviceByDevEuiAsync<T>(string devEui, AuthenticatedUserDto authenticatedUser) where T : class;
        Task<T?> GetDeviceByDevEuiAsync<T>(string devEui) where T : class;
        Task<BatteryResponse> UpdateBatteryAsync(string devEui, float battery, AuthenticatedUserDto authenticatedUser);
        Task<List<DeviceDto>> GetAllDevicesForUserAsync(AuthenticatedUserDto authenticatedUser);

        //Task<bool> UpdateConfigDeviceByDevEuiAsync(DeviceConfigUpdateDTO dto, AuthenticatedUserDto authenticatedUser);

        Task<bool> UpdateConfigDeviceByDevEuiAsync(string devEui, DeviceConfigUpdateDTO dto, AuthenticatedUserDto authenticatedUser);

        Task CreateDeviceAsync(DeviceCreateDto dto, AuthenticatedUserDto user);

    }
}