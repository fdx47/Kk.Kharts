using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService
{
    public interface ISoilParameterService
    {
        Task<SoilParameter> GetSoilParameterByIdAsync(int id) ;
        Task<List<SoilParameter>> GetAllSoilParameterAsync();


    }
}