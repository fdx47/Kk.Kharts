using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IWet150Sdi12MetadataService
    {
        Task<IEnumerable<Wet150Sdi12MetadataResponseDto>> GetByDevEuiAsync(string devEui);

        Task<Wet150Sdi12MetadataResponseDto?> GetByDevEuiAndIndexAsync(string devEui, int index);

        Task CreateAsync(CreateWet150Sdi12MetadataDto dto);

        Task UpdateAsync(int id, UpdateWet150Sdi12MetadataDto dto);

        Task DeleteAsync(int id);
    }

}
