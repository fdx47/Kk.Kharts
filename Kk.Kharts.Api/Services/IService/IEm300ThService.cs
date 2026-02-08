using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Kk.Kharts.Shared.DTOs.Interfaces;
using Kk.Kharts.Shared.Entities.Em300;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IEm300ThService
    {
        Task<Em300ThResponseDTO> GetFilteredDataByDeviEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser);
        //Task<Em300Th> AddApiKeyAsync(Em300ThDTO entity, uint deviceId);
        //Task<Em300Th> AddApiKeyAsync<T>(T entity, int deviceId) where T : class, IDeviceData;
        Task<Em300Th> AddApiKeyAsync<T>(T entity, string devEui) where T : class, IDeviceDataEm300Th;
    }
}
