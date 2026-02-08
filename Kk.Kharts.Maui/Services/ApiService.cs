using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kk.Kharts.Maui.Models;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.DTOs.Companies;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;

namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Implementation of API service with authenticated HTTP calls.
/// </summary>
public sealed class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorageService _secureStorage;
    private readonly IAuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, ISecureStorageService secureStorage, IAuthService authService)
    {
        _httpClient = httpClient;
        _secureStorage = secureStorage;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Devices

    public async Task<Result<List<DeviceDto>>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteAuthenticatedAsync<List<DeviceDto>>(
            () => _httpClient.GetAsync("devices", cancellationToken),
            cancellationToken);
    }

    public async Task<Result<DeviceDto>> GetDeviceAsync(string devEui, CancellationToken cancellationToken = default)
    {
        var encodedDevEui = Uri.EscapeDataString(devEui);
        return await ExecuteAuthenticatedAsync<DeviceDto>(
            () => _httpClient.GetAsync($"devices/{encodedDevEui}?devEui={encodedDevEui}", cancellationToken),
            cancellationToken);
    }

    public async Task<Result> UpdateDeviceConfigAsync(string devEui, DeviceConfigUpdateDTO config, CancellationToken cancellationToken = default)
    {
        var encodedDevEui = Uri.EscapeDataString(devEui);
        return await ExecuteAuthenticatedAsync(
            async () =>
            {
                var content = JsonContent.Create(config, options: _jsonOptions);
                return await _httpClient.PutAsync($"devices/{encodedDevEui}/config", content, cancellationToken);
            },
            cancellationToken);
    }

    #endregion

    #region Sensor Data

    public async Task<Result<Em300ThResponseDTO>> GetEm300ThDataAsync(string devEui, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var encodedDevEui = Uri.EscapeDataString(devEui);
        var start = startDate.ToString("yyyy-MM-dd HH:mm");
        var end = endDate.ToString("yyyy-MM-dd HH:mm");

        return await ExecuteAuthenticatedAsync<Em300ThResponseDTO>(
            () => _httpClient.GetAsync($"em300/{encodedDevEui}/th?startDate={start}&endDate={end}", cancellationToken),
            cancellationToken);
    }

    public async Task<Result<Uc502Wet150ResponseDTO>> GetUc502Wet150DataAsync(string devEui, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var encodedDevEui = Uri.EscapeDataString(devEui);
        var start = startDate.ToString("yyyy-MM-dd HH:mm");
        var end = endDate.ToString("yyyy-MM-dd HH:mm");

        return await ExecuteAuthenticatedAsync<Uc502Wet150ResponseDTO>(
            () => _httpClient.GetAsync($"uc502/{encodedDevEui}/wet150?startDate={start}&endDate={end}", cancellationToken),
            cancellationToken);
    }

    #endregion

    #region Thresholds

    public async Task<Result<List<ThresholdDto>>> GetThresholdsAsync(string devEui, CancellationToken cancellationToken = default)
    {
        var encodedDevEui = Uri.EscapeDataString(devEui);
        return await ExecuteAuthenticatedAsync<List<ThresholdDto>>(
            () => _httpClient.GetAsync($"devices/{encodedDevEui}/thresholds-alarms", cancellationToken),
            cancellationToken);
    }

    public async Task<Result> UpdateThresholdsAsync(List<ThresholdDto> thresholds, CancellationToken cancellationToken = default)
    {
        return await ExecuteAuthenticatedAsync(
            async () =>
            {
                var content = JsonContent.Create(thresholds, options: _jsonOptions);
                return await _httpClient.PostAsync("devices/thresholds-alarms", content, cancellationToken);
            },
            cancellationToken);
    }

    #endregion

    #region Companies

    public async Task<Result<List<CompanyDto>>> GetCompaniesAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteAuthenticatedAsync<List<CompanyDto>>(
            () => _httpClient.GetAsync("companies", cancellationToken),
            cancellationToken);
    }

    #endregion

    #region Private Methods

    private async Task<Result<T>> ExecuteAuthenticatedAsync<T>(
        Func<Task<HttpResponseMessage>> requestFunc,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure token is fresh
            await _authService.RefreshTokenIfNeededAsync(cancellationToken);

            // Get token and set header
            var (accessToken, _, _) = await _secureStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return Result<T>.Failure("Not authenticated");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await requestFunc();

            // Handle 401 - try refresh and retry
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshResult = await _authService.RefreshTokenIfNeededAsync(cancellationToken);
                if (refreshResult.IsFailure)
                {
                    return Result<T>.Failure("Authentication failed");
                }

                var (newToken, _, _) = await _secureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await requestFunc();
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result<T>.Failure($"API error: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
            return result is not null
                ? Result<T>.Success(result)
                : Result<T>.Failure("Empty response");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Request failed: {ex.Message}");
        }
    }

    private async Task<Result> ExecuteAuthenticatedAsync(
        Func<Task<HttpResponseMessage>> requestFunc,
        CancellationToken cancellationToken)
    {
        try
        {
            await _authService.RefreshTokenIfNeededAsync(cancellationToken);

            var (accessToken, _, _) = await _secureStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return Result.Failure("Not authenticated");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await requestFunc();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshResult = await _authService.RefreshTokenIfNeededAsync(cancellationToken);
                if (refreshResult.IsFailure)
                {
                    return Result.Failure("Authentication failed");
                }

                var (newToken, _, _) = await _secureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await requestFunc();
            }

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure($"API error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Request failed: {ex.Message}");
        }
    }

    #endregion
}
