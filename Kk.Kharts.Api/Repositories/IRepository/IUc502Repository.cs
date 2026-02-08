using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Shared.Entities.UC502;
using Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor;

namespace Kk.Kharts.Api.Repositories.IRepository
{
    public interface IUc502Repository
    {
        Task AddEntityAndSaveAsync(Uc502Wet150 entity);
        Task<IQueryable<Uc502Wet150>> GetUc502Wet150DataByDevEuiAsync(string devEui, AuthenticatedUserDto authenticatedUser);
        Task<bool> DeleteWet150ByIdAsync(DateTime timestamp, string devEui);

        Task AddEntityAndSaveModbusDataAsync(Uc502Modbus entity);
        Task<List<Uc502Modbus>> GetUc502ModbusDataByDevEuiApiKeyAsync(string devEui);


        Task AddMultiSensor2Async(Wet150MultiSensor2 entity);
        Task AddMultiSensor3Async(Wet150MultiSensor3 entity);
        Task AddMultiSensor4Async(Wet150MultiSensor4 entity);

        Task<List<Wet150Sdi12Metadata>> GetSdi12MetadataByDevEuiAsync(string devEui);
        Task<List<SensorDataEntity>> GetMultiSensorDataByModelAsync(string devEui, int model);

        Task<Dictionary<string, string?>> GetLastSdi12ValuesAsync(string devEui);
    }
}