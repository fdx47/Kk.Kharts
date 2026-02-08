using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;
using Kk.Kharts.Shared.DTOs.UC502.Modbus;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor;
using Kk.Kharts.Shared.Entities.UC502;

namespace Kk.Kharts.Api.Services.IService
{
    public interface IUc502Service
    {
        // Modbus
        Task<Uc502ModbusResponseDTO> GetFilteredModbusDataByDeviceAsync(string devEui, DateTime startDate, DateTime endDate);
        Task<Uc502Modbus> AddAsyncModbus(Uc502ModbusDataDTO entity);

        // Wet 150
        Task<Uc502Wet150ResponseDTO> GetFilteredDataByDeviEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser);
        Task<Uc502Wet150> AddAsync(Uc502Wet150DTO entity);
        Task<Uc502Wet150> CalculAndAddAsync(PayloadWet150FromUg65WithApiKeyDTO payloadJson, string devEui);
        Task<bool> DeleteWet150ByIdAsync(DateTime timestamp, string devEui, AuthenticatedUserDto authenticatedUser);

        // Wet150 MultiSensor
        Task ProcessPayloadWet150MultiSensorAsync(PayloadWet150MultiSensorFromUg65Dto dto, DeviceDto device);
        Task<Wet150MultiSensorResponseDTO> GetMultiSensor2ByDevEuiAsync(string devEui, DateTime startDate, DateTime endDate, AuthenticatedUserDto authenticatedUser);
       
    }
}
