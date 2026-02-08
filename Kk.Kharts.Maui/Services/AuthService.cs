using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using Kk.Kharts.Maui.Models;
using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Implementation of authentication service.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorageService _secureStorage;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public UserInfo? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public event EventHandler<AuthStateChangedEventArgs>? AuthStateChanged;

    public AuthService(ISecureStorageService secureStorage)
    {
        _secureStorage = secureStorage;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiSettings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(ApiSettings.TimeoutSeconds)
        };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<Result<UserInfo>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var loginDto = new LoginDTO { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("auth/login", loginDto, _jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result<UserInfo>.Failure($"Login failed: {response.StatusCode}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(_jsonOptions, cancellationToken);
            if (authResponse is null || string.IsNullOrEmpty(authResponse.Token))
            {
                return Result<UserInfo>.Failure("Invalid response from server");
            }

            await _secureStorage.SaveTokenAsync(
                authResponse.Token,
                authResponse.RefreshToken,
                authResponse.RefreshTokenExpiryTime);

            var user = ExtractUserFromToken(authResponse.Token);
            CurrentUser = user;

            AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
            {
                IsAuthenticated = true,
                User = user
            });

            return Result<UserInfo>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<UserInfo>.Failure($"Login error: {ex.Message}");
        }
    }

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _secureStorage.ClearTokenAsync();
            CurrentUser = null;

            AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
            {
                IsAuthenticated = false,
                User = null
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Logout error: {ex.Message}");
        }
    }

    public async Task<Result> RefreshTokenIfNeededAsync(CancellationToken cancellationToken = default)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var (accessToken, refreshToken, expiry) = await _secureStorage.GetTokenAsync();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Result.Failure("No tokens stored");
            }

            // Check if refresh token is expired
            if (expiry.HasValue && DateTime.UtcNow >= expiry.Value)
            {
                await _secureStorage.ClearTokenAsync();
                CurrentUser = null;
                AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
                {
                    IsAuthenticated = false,
                    User = null
                });
                return Result.Failure("Refresh token expired");
            }

            // Check if access token needs refresh (5 minutes before expiry)
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(accessToken))
            {
                var jwt = handler.ReadJwtToken(accessToken);
                var tokenExpiry = jwt.ValidTo;

                if (DateTime.UtcNow < tokenExpiry.AddMinutes(-5))
                {
                    return Result.Success(); // Token still valid
                }
            }

            // Refresh the token
            var request = new RefreshTokenRequestDTO { RefreshToken = refreshToken };
            var response = await _httpClient.PostAsJsonAsync("auth/refresh-token", request, _jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await _secureStorage.ClearTokenAsync();
                CurrentUser = null;
                AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
                {
                    IsAuthenticated = false,
                    User = null
                });
                return Result.Failure("Token refresh failed");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(_jsonOptions, cancellationToken);
            if (authResponse is null || string.IsNullOrEmpty(authResponse.Token))
            {
                await _secureStorage.ClearTokenAsync();
                CurrentUser = null;
                AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
                {
                    IsAuthenticated = false,
                    User = null
                });
                return Result.Failure("Invalid refresh response");
            }

            await _secureStorage.SaveTokenAsync(
                authResponse.Token,
                authResponse.RefreshToken,
                authResponse.RefreshTokenExpiryTime);

            CurrentUser = ExtractUserFromToken(authResponse.Token);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Token refresh error: {ex.Message}");
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task<Result<UserInfo>> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var (accessToken, refreshToken, expiry) = await _secureStorage.GetTokenAsync();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Result<UserInfo>.Failure("No stored session");
            }

            // Check if refresh token is expired
            if (expiry.HasValue && DateTime.UtcNow >= expiry.Value)
            {
                await _secureStorage.ClearTokenAsync();
                return Result<UserInfo>.Failure("Session expired");
            }

            // Try to refresh if needed
            var refreshResult = await RefreshTokenIfNeededAsync(cancellationToken);
            if (refreshResult.IsFailure)
            {
                return Result<UserInfo>.Failure(refreshResult.Error ?? "Session restore failed");
            }

            var (newAccessToken, _, _) = await _secureStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(newAccessToken))
            {
                return Result<UserInfo>.Failure("No access token after refresh");
            }

            var user = ExtractUserFromToken(newAccessToken);
            CurrentUser = user;

            AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
            {
                IsAuthenticated = true,
                User = user
            });

            return Result<UserInfo>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<UserInfo>.Failure($"Session restore error: {ex.Message}");
        }
    }

    private static UserInfo ExtractUserFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var userId = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value ?? "";
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
        var role = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "";
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;

        return new UserInfo
        {
            Id = userId,
            Email = email,
            Role = role,
            Name = name
        };
    }
}
