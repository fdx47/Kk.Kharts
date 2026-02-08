using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kk.Kharts.Maui.Models;
using Kk.Kharts.Maui.Services;

namespace Kk.Kharts.Maui.ViewModels;

/// <summary>
/// ViewModel for the dashboard page showing device overview grouped by company.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<CompanyGroup> _displayGroups = [];

    [ObservableProperty]
    private DeviceModel? _selectedDevice;

    [ObservableProperty]
    private int _totalDevices;

    [ObservableProperty]
    private int _activeDevices;

    [ObservableProperty]
    private int _lowBatteryDevices;

    [ObservableProperty]
    private int _totalCompanies;

    [ObservableProperty]
    private bool _showGroupedView = true;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    private List<DeviceModel> _allDevices = [];

    public DashboardViewModel(
        IApiService apiService,
        INavigationService navigationService,
        IAuthService authService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _authService = authService;
        Title = "Dashboard";
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterDevices();
    }

    partial void OnShowGroupedViewChanged(bool value)
    {
        FilterDevices();
    }

    [RelayCommand]
    private async Task LoadDevicesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _apiService.GetDevicesAsync();

            if (result.IsSuccess)
            {
                var payload = result.ValueOrDefault ?? [];

                _allDevices = payload
                    .Select(DeviceModel.FromDto)
                    .OrderBy(d => d.CompanyName)
                    .ThenBy(d => d.Name)
                    .ToList();

                if (_allDevices.Count == 0)
                {
                    SetError("Aucun kapteur trouvé pour ce compte.");
                }
                else
                {
                    UpdateStats();
                    FilterDevices();
                }
            }
            else
            {
                SetError(result.Error ?? "Erreur lors du chargement des capteurs");
            }
        }, "Erreur lors du chargement");
    }

    private void UpdateStats()
    {
        TotalDevices = _allDevices.Count;
        ActiveDevices = _allDevices.Count(d => d.ActiveInKropKontrol == true);
        LowBatteryDevices = _allDevices.Count(d => d.Battery < 3.0f);
        TotalCompanies = _allDevices
            .Select(d => string.IsNullOrWhiteSpace(d.CompanyName) ? "Sites non assignés" : d.CompanyName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private void FilterDevices()
    {
        var query = SearchQuery?.Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? _allDevices
            : _allDevices.Where(d => MatchesSearch(d, query)).ToList();

        List<CompanyGroup> groups;

        if (ShowGroupedView)
        {
            groups = filtered
                .GroupBy(d => string.IsNullOrWhiteSpace(d.CompanyName) ? "Sites non assignés" : d.CompanyName)
                .OrderBy(g => g.Key)
                .Select(g => new CompanyGroup(g.Key, g.OrderBy(d => d.Name)))
                .ToList();
        }
        else
        {
            // Single group for flat view
            groups = [new CompanyGroup("Tous les capteurs", filtered)];
        }

        DisplayGroups = new ObservableCollection<CompanyGroup>(groups);
    }

    private static bool MatchesSearch(DeviceModel device, string query)
    {
        return Contains(device.Name, query)
            || Contains(device.CompanyName, query)
            || Contains(device.InstallationLocation, query)
            || Contains(device.DevEui, query);
    }

    private static bool Contains(string? source, string query)
        => !string.IsNullOrEmpty(source)
            && source.Contains(query, StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private void ToggleView()
    {
        ShowGroupedView = !ShowGroupedView;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDevicesAsync();
    }

    [RelayCommand]
    private async Task SelectDeviceAsync(DeviceModel? device)
    {
        if (device is null) return;

        SelectedDevice = device;
        await _navigationService.NavigateToChartAsync(device.DevEui);
    }

    [RelayCommand]
    private async Task ViewChartAsync(DeviceModel? device)
    {
        if (device is null) return;

        await _navigationService.NavigateToChartAsync(device.DevEui);
    }

    [RelayCommand]
    private async Task ViewDeviceDetailAsync(DeviceModel? device)
    {
        if (device is null) return;

        await _navigationService.NavigateToDeviceDetailAsync(device.DevEui);
    }

    public void OnAppearing()
    {
        if (DisplayGroups.Count == 0)
        {
            _ = LoadDevicesAsync();
        }
    }
}
