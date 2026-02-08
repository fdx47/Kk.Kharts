namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Implementation of connectivity service using MAUI Connectivity.
/// </summary>
public sealed class ConnectivityService : IConnectivityService, IDisposable
{
    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<bool>? ConnectivityChanged;

    public ConnectivityService()
    {
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        ConnectivityChanged?.Invoke(this, e.NetworkAccess == NetworkAccess.Internet);
    }

    public void Dispose()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }
}
