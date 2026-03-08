using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.UC502;

namespace Kk.Kharts.Api.Services.IService;

public interface IWet150MulticapteurService
{
    Task TraiterAsync(PayloadWet150MultiSensorFromUg65Dto chargeUtile, DeviceDto appareil, string endpoint);
}
