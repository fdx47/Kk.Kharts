using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kk.Kharts.Maui.Models;
using Kk.Kharts.Maui.Services;
using Kk.Kharts.Shared.DTOs;

namespace Kk.Kharts.Maui.ViewModels;

/// <summary>
/// ViewModel for device detail page.
/// </summary>
[QueryProperty(nameof(DevEui), "DevEui")]
public partial class DeviceDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _devEui = string.Empty;

    [ObservableProperty]
    private DeviceModel? _device;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _installationLocation = string.Empty;

    [ObservableProperty]
    private bool _isActive;

    public DeviceDetailViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        Title = "Détails du Kapteur";
    }

    partial void OnDevEuiChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadDeviceAsync();
        }
    }

    [RelayCommand]
    private async Task LoadDeviceAsync()
    {
        if (string.IsNullOrEmpty(DevEui)) return;

        await ExecuteAsync(async () =>
        {
            var result = await _apiService.GetDeviceAsync(DevEui);

            if (result.IsSuccess)
            {
                Device = DeviceModel.FromDto(result.Value);
                Name = Device.Name;
                Description = Device.Description;
                InstallationLocation = Device.InstallationLocation ?? string.Empty;
                IsActive = Device.ActiveInKropKontrol ?? false;
                Title = Device.Name;
            }
            else
            {
                SetError(result.Error ?? "Erreur lors du chargement");
            }
        });
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (Device is null) return;

        await ExecuteAsync(async () =>
        {
            var config = new DeviceConfigUpdateDTO
            {
                Name = Name,
                Description = Description,
                InstallationLocation = InstallationLocation,
                ActiveInKropKontrol = IsActive
            };

            var result = await _apiService.UpdateDeviceConfigAsync(DevEui, config);

            if (result.IsSuccess)
            {
                await Shell.Current.DisplayAlertAsync("Succès", "Configuration enregistrée", "OK");
            }
            else
            {
                SetError(result.Error ?? "Erreur lors de l'enregistrement");
            }
        });
    }

    [RelayCommand]
    private async Task ViewChartAsync()
    {
        if (Device is null) return;
        await _navigationService.NavigateToChartAsync(Device.DevEui);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToMainAsync();
    }
}
