namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Interface for secure storage of sensitive data.
/// </summary>
public interface ISecureStorageService
{
    Task SaveTokenAsync(string accessToken, string refreshToken, DateTime refreshTokenExpiry);
    Task<(string? AccessToken, string? RefreshToken, DateTime? RefreshTokenExpiry)> GetTokenAsync();
    Task ClearTokenAsync();
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}
