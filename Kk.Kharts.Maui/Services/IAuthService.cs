using Kk.Kharts.Maui.Models;

namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Interface for authentication operations.
/// </summary>
public interface IAuthService
{
    UserInfo? CurrentUser { get; }
    bool IsAuthenticated { get; }
    
    Task<Result<UserInfo>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
    Task<Result> RefreshTokenIfNeededAsync(CancellationToken cancellationToken = default);
    Task<Result<UserInfo>> TryRestoreSessionAsync(CancellationToken cancellationToken = default);
    
    event EventHandler<AuthStateChangedEventArgs>? AuthStateChanged;
}

public class AuthStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; init; }
    public UserInfo? User { get; init; }
}
