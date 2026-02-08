using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Em300.Em300Di;
using Kk.Kharts.Shared.DTOs.Interfaces;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IEm300DiService
    {
        Task<Em300DiResponseDTO> GetFilteredDataByDeviEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser);
        Task<Em300Di> AddApiKeyAsync<T>(T entity, string devEui) where T : class, IDeviceDataEm300Di;
    }
}
