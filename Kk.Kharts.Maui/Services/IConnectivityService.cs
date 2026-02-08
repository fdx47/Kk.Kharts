namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Interface for connectivity status.
/// </summary>
public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<bool>? ConnectivityChanged;
}
