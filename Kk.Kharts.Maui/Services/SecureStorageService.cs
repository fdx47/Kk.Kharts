namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Implementation of secure storage using MAUI SecureStorage.
/// </summary>
public sealed class SecureStorageService : ISecureStorageService
{
    private const string AccessTokenKey = "kk_access_token";
    private const string RefreshTokenKey = "kk_refresh_token";
    private const string RefreshTokenExpiryKey = "kk_refresh_token_expiry";

    public async Task SaveTokenAsync(string accessToken, string refreshToken, DateTime refreshTokenExpiry)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        await SecureStorage.Default.SetAsync(RefreshTokenExpiryKey, refreshTokenExpiry.ToString("O"));
    }

    public async Task<(string? AccessToken, string? RefreshToken, DateTime? RefreshTokenExpiry)> GetTokenAsync()
    {
        var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
        var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        var expiryStr = await SecureStorage.Default.GetAsync(RefreshTokenExpiryKey);

        DateTime? expiry = null;
        if (!string.IsNullOrEmpty(expiryStr) && DateTime.TryParse(expiryStr, out var parsed))
        {
            expiry = parsed;
        }

        return (accessToken, refreshToken, expiry);
    }

    public Task ClearTokenAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(RefreshTokenExpiryKey);
        return Task.CompletedTask;
    }

    public Task SetAsync(string key, string value)
    {
        return SecureStorage.Default.SetAsync(key, value);
    }

    public Task<string?> GetAsync(string key)
    {
        return SecureStorage.Default.GetAsync(key);
    }

    public Task RemoveAsync(string key)
    {
        SecureStorage.Default.Remove(key);
        return Task.CompletedTask;
    }
}
