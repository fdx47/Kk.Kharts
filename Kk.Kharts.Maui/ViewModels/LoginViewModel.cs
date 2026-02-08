using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kk.Kharts.Maui.Services;

namespace Kk.Kharts.Maui.ViewModels;

/// <summary>
/// ViewModel for the login page.
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IConnectivityService _connectivityService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public LoginViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IConnectivityService connectivityService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _connectivityService = connectivityService;
        Title = "Connexion";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Veuillez saisir votre email et mot de passe");
            return;
        }

        if (!_connectivityService.IsConnected)
        {
            SetError("Pas de connexion internet");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await _authService.LoginAsync(Email, Password);

            if (result.IsSuccess)
            {
                // Clear sensitive data
                Password = string.Empty;
                await _navigationService.NavigateToMainAsync();
            }
            else
            {
                SetError(result.Error ?? "Identifiants invalides");
            }
        }, "Erreur de connexion");
    }

    partial void OnEmailChanged(string value)
    {
        ClearError();
    }

    partial void OnPasswordChanged(string value)
    {
        ClearError();
    }
}
