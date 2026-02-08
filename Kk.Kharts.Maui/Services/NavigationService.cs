namespace Kk.Kharts.Maui.Services;

/// <summary>
/// Implementation of navigation service using Shell navigation.
/// </summary>
public sealed class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters is not null)
        {
            await Shell.Current.GoToAsync(route, parameters);
        }
        else
        {
            await Shell.Current.GoToAsync(route);
        }
    }

    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }

    public async Task NavigateToMainAsync()
    {
        // Navigate to dashboard tab in main shell
        await Shell.Current.GoToAsync("//main/dashboard");
    }

    public async Task NavigateToDeviceDetailAsync(string devEui)
    {
        await Shell.Current.GoToAsync("deviceDetail", new Dictionary<string, object>
        {
            { "DevEui", devEui }
        });
    }

    public async Task NavigateToChartAsync(string devEui)
    {
        await Shell.Current.GoToAsync("chart", new Dictionary<string, object>
        {
            { "DevEui", devEui }
        });
    }
}
