using Kk.Kharts.Maui.Services;
using Kk.Kharts.Maui.Views;
using Microsoft.Maui.ApplicationModel;

namespace Kk.Kharts.Maui;

public partial class App : Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Try to restore session on app start
        var result = await _authService.TryRestoreSessionAsync();
        if (result.IsSuccess)
        {
            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            await Shell.Current.GoToAsync("//login");
        }
    }

    private void OnAuthStateChanged(object? sender, AuthStateChangedEventArgs e)
    {
        if (e.IsAuthenticated)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("//login");
            }
        });
    }
}
