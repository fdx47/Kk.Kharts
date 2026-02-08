using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kk.Kharts.Maui.Models;
using Kk.Kharts.Maui.Services;
using DeviceType = Kk.Kharts.Maui.Models.DeviceType;

namespace Kk.Kharts.Maui.ViewModels;

/// <summary>
/// ViewModel for the devices list page.
/// </summary>
public partial class DevicesViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    private List<DeviceModel> _allDevices = [];

    [ObservableProperty]
    private ObservableCollection<DeviceModel> _devices = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DeviceType? _filterType;

    [ObservableProperty]
    private bool _isRefreshing;

    public DevicesViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        Title = "Kapteurs";
    }

    [RelayCommand]
    private async Task LoadDevicesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _apiService.GetDevicesAsync();

            if (result.IsSuccess)
            {
                _allDevices = result.Value
                    .Select(DeviceModel.FromDto)
                    .OrderBy(d => d.Name)
                    .ToList();

                ApplyFilters();
            }
            else
            {
                SetError(result.Error ?? "Erreur lors du chargement");
            }
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadDevicesAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task SelectDeviceAsync(DeviceModel? device)
    {
        if (device is null) return;
        await _navigationService.NavigateToDeviceDetailAsync(device.DevEui);
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnFilterTypeChanged(DeviceType? value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allDevices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(d =>
                d.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.DevEui.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (d.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.InstallationLocation?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (FilterType.HasValue)
        {
            filtered = filtered.Where(d => d.DeviceType == FilterType.Value);
        }

        Devices = new ObservableCollection<DeviceModel>(filtered);
    }

    public void OnAppearing()
    {
        if (_allDevices.Count == 0)
        {
            _ = LoadDevicesAsync();
        }
    }
}
