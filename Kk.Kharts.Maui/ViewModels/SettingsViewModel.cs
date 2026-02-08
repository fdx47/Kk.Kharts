using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kk.Kharts.Maui.Models;
using Kk.Kharts.Maui.Services;

namespace Kk.Kharts.Maui.ViewModels;

/// <summary>
/// ViewModel for settings page.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private UserInfo? _currentUser;

    [ObservableProperty]
    private string _appVersion = "0.0.1b";

    public SettingsViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "À propos";

        CurrentUser = _authService.CurrentUser;
        AppVersion = AppInfo.Current.VersionString;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Déconnexion",
            "Voulez-vous vraiment vous déconnecter ?",
            "Oui",
            "Non");

        if (!confirm) return;

        await ExecuteAsync(async () =>
        {
            var result = await _authService.LogoutAsync();
            if (result.IsSuccess)
            {
                await _navigationService.NavigateToLoginAsync();
            }
            else
            {
                SetError(result.Error ?? "Erreur lors de la déconnexion");
            }
        });
    }

    [RelayCommand]
    private async Task OpenSupportAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("mailto:info@kropkontrol.com"));
        }
        catch
        {
            await Shell.Current.DisplayAlertAsync("Erreur", "Impossible d'ouvrir l'application email", "OK");
        }
    }

    [RelayCommand]
    private async Task OpenWebsiteAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("https://kropkontrol.com"));
        }
        catch
        {
            await Shell.Current.DisplayAlertAsync("Erreur", "Impossible d'ouvrir le navigateur", "OK");
        }
    }

    public void OnAppearing()
    {
        CurrentUser = _authService.CurrentUser;
    }
}
