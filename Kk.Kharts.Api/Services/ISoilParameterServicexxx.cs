using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Api.Services.IService
{
    public interface ISoilParameterServicexxx
    {
        Task<T?> GetDeviceByDevEuiAsync<T>(string devEui, AuthenticatedUserDto authenticatedUser) where T : class;
        Task<T?> GetDeviceByDevEuiAsync<T>(string devEui) where T : class;
       

    }
}