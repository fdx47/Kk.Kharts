namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Interface for navigation operations.
/// </summary>
public interface INavigationService
{
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();
    Task NavigateToLoginAsync();
    Task NavigateToMainAsync();
    Task NavigateToDeviceDetailAsync(string devEui);
    Task NavigateToChartAsync(string devEui);
}
