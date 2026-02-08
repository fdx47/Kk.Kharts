using Kk.Kharts.Maui.Models;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Companies;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;

namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Interface for API operations.
/// </summary>
public interface IApiService
{
    // Devices
    Task<Result<List<DeviceDto>>> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task<Result<DeviceDto>> GetDeviceAsync(string devEui, CancellationToken cancellationToken = default);
    Task<Result> UpdateDeviceConfigAsync(string devEui, DeviceConfigUpdateDTO config, CancellationToken cancellationToken = default);
    
    // Sensor Data
    Task<Result<Em300ThResponseDTO>> GetEm300ThDataAsync(string devEui, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Result<Uc502Wet150ResponseDTO>> GetUc502Wet150DataAsync(string devEui, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // Thresholds
    Task<Result<List<ThresholdDto>>> GetThresholdsAsync(string devEui, CancellationToken cancellationToken = default);
    Task<Result> UpdateThresholdsAsync(List<ThresholdDto> thresholds, CancellationToken cancellationToken = default);
    
    // Companies
    Task<Result<List<CompanyDto>>> GetCompaniesAsync(CancellationToken cancellationToken = default);
}
