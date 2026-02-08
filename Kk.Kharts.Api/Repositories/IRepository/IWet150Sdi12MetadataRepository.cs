using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface IWet150Sdi12MetadataRepository
    {
        Task<IEnumerable<Wet150Sdi12Metadata>> GetByDevEuiAsync(string devEui);
        Task<Wet150Sdi12Metadata?> GetByDevEuiAndIndexAsync(string devEui, int index);

        Task AddAsync(Wet150Sdi12Metadata entity);
        Task<List<Wet150Sdi12Metadata>> GetAllAsync();
        Task<Wet150Sdi12Metadata?> GetByIdAsync(int id);
        Task UpdateAsync(Wet150Sdi12Metadata entity);
        Task DeleteAsync(int id);

    }
}
