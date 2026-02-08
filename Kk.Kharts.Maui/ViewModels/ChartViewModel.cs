using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kk.Kharts.Maui.Models;
using Kk.Kharts.Maui.Services;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Kk.Kharts.Shared.DTOs;
using DeviceType = Kk.Kharts.Maui.Models.DeviceType;

namespace Kk.Kharts.Maui.ViewModels;

/// <summary>
/// ViewModel for chart display page.
/// </summary>
[QueryProperty(nameof(DevEui), "DevEui")]
public partial class ChartViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly List<LineSeries<DateTimePoint>> _rawSeries = new();
    private bool _suppressDevEuiChange;

    private const int DefaultRangeDays = 3;

    [ObservableProperty] private string _devEui = string.Empty;
    [ObservableProperty] private DeviceModel? _device;
    [ObservableProperty] private DateTime _startDate = DateTime.Now.AddDays(-DefaultRangeDays);
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextPeriodCommand))]
    private DateTime _endDate = DateTime.Now;
    [ObservableProperty] private ISeries[] _series = [];
    [ObservableProperty] private Axis[] _xAxes = [];
    [ObservableProperty] private Axis[] _yAxes = [];
    [ObservableProperty] private string _selectedVariable = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _installationLocation = string.Empty;
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private double? _customMinY;
    [ObservableProperty] private double? _customMaxY;
    [ObservableProperty] private bool _useCustomRange;
    [ObservableProperty] private bool _isConfigVisible;

    public ObservableCollection<ChartVariableOption> VariableOptions { get; } = new();

    public ChartViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        Title = "Graphique";

        ResetDateRange();
        InitializeAxes();
    }

    private void InitializeAxes()
    {
        XAxes =
        [
            new DateTimeAxis(TimeSpan.FromHours(1), date => date.ToString("dd/MM HH:mm"))
            {
                //Name = "Date",
                NamePaint = new SolidColorPaint(new SKColor(120, 144, 156)),
                LabelsPaint = new SolidColorPaint(new SKColor(120, 144, 156))
            }
        ];

        YAxes =
        [
            new Axis
            {
                //Name = "Valeurs",
                NamePaint = new SolidColorPaint(new SKColor(44, 62, 80)),
                LabelsPaint = new SolidColorPaint(new SKColor(120, 144, 156))
            }
        ];
    }

    private List<DeviceDto> _allDevices = new();

    public async Task InitializeAsync()
    {
        // Always reset to 3-day default range
        ResetDateRange();
        
        await LoadAllDevicesAsync();

        if (string.IsNullOrWhiteSpace(DevEui) && _allDevices.Count > 0)
        {
            _suppressDevEuiChange = true;
            DevEui = _allDevices[0].DevEui;
            _suppressDevEuiChange = false;
        }

        await LoadDeviceAndDataAsync();
    }

    private async Task LoadAllDevicesAsync()
    {
        var result = await _apiService.GetDevicesAsync();
        if (result.IsSuccess)
        {
            _allDevices = result.Value;
            PreviousDeviceCommand.NotifyCanExecuteChanged();
            NextDeviceCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecutePreviousDevice))]
    private async Task PreviousDeviceAsync()
    {
        if (_allDevices.Count == 0) return;
        
        var currentIndex = _allDevices.FindIndex(d => d.DevEui.Equals(DevEui, StringComparison.OrdinalIgnoreCase));
        if (currentIndex <= 0) return;

        var prevDevice = _allDevices[currentIndex - 1];
        DevEui = prevDevice.DevEui;
    }

    private bool CanExecutePreviousDevice()
    {
        if (_allDevices.Count == 0) return false;
        var currentIndex = _allDevices.FindIndex(d => d.DevEui.Equals(DevEui, StringComparison.OrdinalIgnoreCase));
        return currentIndex > 0;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteNextDevice))]
    private async Task NextDeviceAsync()
    {
        if (_allDevices.Count == 0) return;

        var currentIndex = _allDevices.FindIndex(d => d.DevEui.Equals(DevEui, StringComparison.OrdinalIgnoreCase));
        if (currentIndex < 0 || currentIndex >= _allDevices.Count - 1) return;

        var nextDevice = _allDevices[currentIndex + 1];
        DevEui = nextDevice.DevEui;
    }

    private bool CanExecuteNextDevice()
    {
        if (_allDevices.Count == 0) return false;
        var currentIndex = _allDevices.FindIndex(d => d.DevEui.Equals(DevEui, StringComparison.OrdinalIgnoreCase));
        return currentIndex >= 0 && currentIndex < _allDevices.Count - 1;
    }

    partial void OnDevEuiChanged(string value)
    {
        if (_suppressDevEuiChange || string.IsNullOrWhiteSpace(value))
            return;

        _ = LoadDeviceAndDataAsync();
        
        // Update commands state
        PreviousDeviceCommand.NotifyCanExecuteChanged();
        NextDeviceCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task LoadDeviceAndDataAsync()
    {
        if (string.IsNullOrWhiteSpace(DevEui) || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var deviceResult = await _apiService.GetDeviceAsync(DevEui);
            if (deviceResult.IsFailure)
            {
                var errorMsg = deviceResult.Error ?? "Erreur lors du chargement";
                if (errorMsg.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
                {
                    errorMsg = "Accès refusé à ce capteur. Vérifiez vos permissions.";
                }
                SetError(errorMsg);
                return;
            }

            Device = DeviceModel.FromDto(deviceResult.Value);
            //Title = Device.Name;
            Name = Device.Name;
            Description = Device.Description;
            InstallationLocation = Device.InstallationLocation ?? string.Empty;
            IsActive = Device.ActiveInKropKontrol ?? false;

            SetAvailableVariables();
            await LoadChartDataAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetAvailableVariables()
    {
        foreach (var option in VariableOptions)
        {
            option.PropertyChanged -= OnVariableOptionChanged;
        }

        VariableOptions.Clear();

        switch (Device?.DeviceType)
        {
            case DeviceType.Em300Th:
                AddVariableOptions(["Temperature", "Humidity", "Battery"]);
                break;

            case DeviceType.Uc502Wet150:
            case DeviceType.Uc502MultiSensor:
                AddVariableOptions([
                    "SoilTemperature", "Permittivite", "MineralVWC", "OrganicVWC",
                    "PeatMixVWC", "CoirVWC", "MinWoolVWC", "PerliteVWC",
                    "MineralECp", "OrganicECp", "Battery"
                ]);
                break;

            default:
                AddVariableOptions(["Battery"]);
                break;
        }

        SelectedVariable = VariableOptions.FirstOrDefault()?.Key ?? string.Empty;
        
        // Refresh chart with new options if we have data
        if (_rawSeries.Count > 0)
        {
            ApplySeriesFilter();
        }
    }

    private void AddVariableOptions(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            var color = GetVariableColor(key);
            var option = new ChartVariableOption(
                key,
                GetVariableLabel(key),
                Color.FromRgba(color.Red, color.Green, color.Blue, color.Alpha),
                color,
                isVisible: true);

            option.PropertyChanged += OnVariableOptionChanged;
            VariableOptions.Add(option);
        }
    }

    private void OnVariableOptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        ApplySeriesFilter();
    }

    [RelayCommand]
    private async Task LoadChartDataAsync()
    {
        if (Device is null)
            return;

        await ExecuteAsync(async () =>
        {
            switch (Device.DeviceType)
            {
                case DeviceType.Em300Th:
                    await LoadEm300ThDataAsync();
                    break;

                case DeviceType.Uc502Wet150:
                case DeviceType.Uc502MultiSensor:
                    await LoadUc502Wet150DataAsync();
                    break;

                default:
                    SetError("Type de capteur non supporté");
                    break;
            }
        });
    }

    private async Task LoadEm300ThDataAsync()
    {
        var result = await _apiService.GetEm300ThDataAsync(DevEui, StartDate, EndDate);
        if (result.IsFailure)
        {
            SetError(result.Error ?? "Erreur lors du chargement des données");
            return;
        }

        var data = result.Value.Data;
        var seriesList = new List<LineSeries<DateTimePoint>>
        {
            CreateLineSeries("Temperature", "Température (°C)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.Temperature, 1))), GetVariableColor("Temperature")),
            CreateLineSeries("Humidity", "Humidité (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.Humidity))), GetVariableColor("Humidity")),
            CreateLineSeries("Battery", "Batterie (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.Battery))), GetVariableColor("Battery"))
        };

        UpdateChartSeries(seriesList);
    }

    private async Task LoadUc502Wet150DataAsync()
    {
        var result = await _apiService.GetUc502Wet150DataAsync(DevEui, StartDate, EndDate);
        if (result.IsFailure)
        {
            SetError(result.Error ?? "Erreur lors du chargement des données");
            return;
        }

        var data = result.Value.Data;
        var seriesList = new List<LineSeries<DateTimePoint>>
        {
            CreateLineSeries("SoilTemperature", "Température Sol (°C)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.SoilTemperature, 1))), GetVariableColor("SoilTemperature")),
            CreateLineSeries("Permittivite", "Permittivité", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.Permittivite, 2))), GetVariableColor("Permittivite")),
            CreateLineSeries("MineralVWC", "VWC Minérale (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.MineralVWC, 1))), GetVariableColor("MineralVWC")),
            CreateLineSeries("OrganicVWC", "VWC Organique (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.OrganicVWC, 1))), GetVariableColor("OrganicVWC")),
            CreateLineSeries("PeatMixVWC", "VWC Tourbe (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.PeatMixVWC, 1))), GetVariableColor("PeatMixVWC")),
            CreateLineSeries("CoirVWC", "VWC Coco (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.CoirVWC, 1))), GetVariableColor("CoirVWC")),
            CreateLineSeries("MinWoolVWC", "VWC Laine Minérale (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.MinWoolVWC, 1))), GetVariableColor("MinWoolVWC")),
            CreateLineSeries("PerliteVWC", "VWC Perlite (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.PerliteVWC, 1))), GetVariableColor("PerliteVWC")),
            CreateLineSeries("MineralECp", "EC Minérale (mS/cm)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.MineralECp, 2))), GetVariableColor("MineralECp")),
            CreateLineSeries("OrganicECp", "EC Organique (mS/cm)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.OrganicECp, 2))), GetVariableColor("OrganicECp")),
            CreateLineSeries("Battery", "Batterie (%)", data.Select(d => new DateTimePoint(d.Timestamp, Math.Round(d.Battery))), GetVariableColor("Battery"))
        };

        UpdateChartSeries(seriesList);
    }

    private void UpdateChartSeries(List<LineSeries<DateTimePoint>> seriesList)
    {
        _rawSeries.Clear();
        _rawSeries.AddRange(seriesList);
        ApplySeriesFilter();
    }

    private void ApplySeriesFilter()
    {
        if (_rawSeries.Count == 0)
        {
            Series = Array.Empty<ISeries>();
            YAxes = Array.Empty<Axis>();
            return;
        }

        var visibleOptions = VariableOptions.Where(v => v.IsVisible).ToList();
        var visibleKeys = visibleOptions.Select(v => v.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        var activeSeries = _rawSeries
            .Where(s => s.Tag is string key && visibleKeys.Contains(key))
            .Cast<LineSeries<DateTimePoint>>()
            .ToList();

        var axes = new List<Axis>();
        var defaultAxis = new Axis
        {
            //Name = "Valeurs",
            NamePaint = new SolidColorPaint(new SKColor(44, 62, 80)),
            LabelsPaint = new SolidColorPaint(new SKColor(120, 144, 156)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 128)),
            ShowSeparatorLines = true,
            MinLimit = UseCustomRange ? CustomMinY : null,
            MaxLimit = UseCustomRange ? CustomMaxY : null
        };
        axes.Add(defaultAxis);

        foreach (var series in activeSeries)
        {
            if (series.Tag is not string key) continue;
            
            var option = visibleOptions.FirstOrDefault(o => o.Key == key);
            if (option is null) continue;

            // Update Color
            if (series.Stroke is SolidColorPaint paint)
            {
                paint.Color = option.AccentSkColor;
            }
            series.Stroke = new SolidColorPaint(option.AccentSkColor, 3);
            series.GeometryStroke = new SolidColorPaint(option.AccentSkColor, 3);
            series.GeometryFill = new SolidColorPaint(SKColors.White);

            // Handle Custom Axis
            if (option.Min.HasValue || option.Max.HasValue)
            {
                var customAxis = new Axis
                {
                    Name = option.Label,
                    NamePaint = new SolidColorPaint(option.AccentSkColor),
                    LabelsPaint = new SolidColorPaint(option.AccentSkColor),
                    ShowSeparatorLines = false, // Avoid clutter
                    MinLimit = option.Min,
                    MaxLimit = option.Max,
                    Position = LiveChartsCore.Measure.AxisPosition.End
                };
                axes.Add(customAxis);
                series.ScalesYAt = axes.Count - 1;
            }
            else
            {
                series.ScalesYAt = 0;
            }
        }

        YAxes = axes.ToArray();
        Series = activeSeries.Cast<ISeries>().ToArray();
        
        // Force chart update
        OnPropertyChanged(nameof(Series));
        OnPropertyChanged(nameof(YAxes));
    }

    public List<Color> AvailableColors { get; } = SeriesColors.Select(c => Color.FromRgba(c.Red, c.Green, c.Blue, c.Alpha)).ToList();

    [RelayCommand]
    private void ToggleColorPalette(ChartVariableOption? option)
    {
        if (option is null) return;
        option.IsColorPaletteVisible = !option.IsColorPaletteVisible;
    }

    [RelayCommand]
    private void SelectColor(ChartVariableOption? option)
    {
        // This command might be called with a tuple or we need a way to pass both option and color.
        // Simpler: The UI binds to the option, and the command parameter is the Color.
        // But we need to know WHICH option to update.
        // Let's use a wrapper or pass the option as CommandParameter to the list, and the color... wait.
        // The CollectionView is inside the DataTemplate of the Option.
        // So the BindingContext of the color item is the Color itself.
        // We need to pass the Option (ancestor) and the Color (current).
        // Actually, simpler: The SelectColor command can be on the ViewModel, taking a wrapper object, OR
        // we can just handle it in the View? No, MVVM.
        
        // Let's define a helper class or just use a CommandParameter that contains both if possible.
        // Or, since we are in the ViewModel, we can just update the option if we knew which one.
        
        // Alternative: The `AvailableColors` is just a list of Colors.
        // In the UI, we iterate over AvailableColors.
        // When clicked, we fire a command.
        
        // Let's try to pass the Option as the CommandParameter, and the Color... how?
        // Maybe we don't need a command for the color selection if we bind it?
        // No, we need to update the SKColor too.
    }

    [RelayCommand]
    private void SetColor(object? parameter)
    {
        if (parameter is not object[] args || args.Length != 2) return;
        if (args[0] is not ChartVariableOption option || args[1] is not Color color) return;

        option.AccentColor = color;
        option.AccentSkColor = new SKColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
        option.IsColorPaletteVisible = false;
        ApplySeriesFilter();
    }

    [RelayCommand]
    private async Task OpenCustomColorPicker(ChartVariableOption? option)
    {
        if (option is null) return;

        // Create a simple prompt for color input (hex format)
        var result = await Shell.Current.DisplayPromptAsync(
            "Couleur personnalisée",
            "Entrez une couleur au format hexadécimal (ex: #FF5733):",
            "OK",
            "Annuler",
            placeholder: "#RRGGBB",
            maxLength: 7,
            keyboard: Keyboard.Text,
            initialValue: $"#{option.AccentSkColor.Red:X2}{option.AccentSkColor.Green:X2}{option.AccentSkColor.Blue:X2}");

        if (string.IsNullOrWhiteSpace(result)) return;

        try
        {
            var color = Color.FromArgb(result);
            option.AccentColor = color;
            option.AccentSkColor = new SKColor((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
            option.IsColorPaletteVisible = false;
            ApplySeriesFilter();
        }
        catch
        {
            await Shell.Current.DisplayAlertAsync("Erreur", "Format de couleur invalide. Utilisez le format #RRGGBB", "OK");
        }
    }

    private static LineSeries<DateTimePoint> CreateLineSeries(string key, string label, IEnumerable<DateTimePoint> points, SKColor color)
        => new()
        {
            Name = label,
            Values = points.ToList(),
            Fill = null,
            GeometrySize = 0,
            Stroke = new SolidColorPaint(color, 3),
            LineSmoothness = 0.6,
            Tag = key
        };

    [RelayCommand]
    private void ApplyCustomRange()
    {
        if (CustomMinY.HasValue && CustomMaxY.HasValue && CustomMinY.Value > CustomMaxY.Value)
        {
            SetError("Le minimum doit être inférieur au maximum.");
            return;
        }

        UseCustomRange = CustomMinY.HasValue || CustomMaxY.HasValue;
        ApplySeriesFilter();
    }

    [RelayCommand]
    private void ResetCustomRange()
    {
        UseCustomRange = false;
        CustomMinY = null;
        CustomMaxY = null;
        ApplySeriesFilter();
    }

    private static double GetEm300ThValue(Em300ThDataDTO data, string variable) => variable switch
    {
        "Temperature" => data.Temperature,
        "Humidity" => data.Humidity,
        "Battery" => Math.Round(data.Battery),
        _ => 0
    };

    private static double GetUc502Value(Uc502Wet150DTO data, string variable) => variable switch
    {
        "SoilTemperature" => data.SoilTemperature,
        "Permittivite" => data.Permittivite,
        "MineralVWC" => data.MineralVWC,
        "OrganicVWC" => data.OrganicVWC,
        "PeatMixVWC" => data.PeatMixVWC,
        "CoirVWC" => data.CoirVWC,
        "MinWoolVWC" => data.MinWoolVWC,
        "PerliteVWC" => data.PerliteVWC,
        "MineralECp" => data.MineralECp,
        "OrganicECp" => data.OrganicECp,
        "Battery" => Math.Round(data.Battery),
        _ => 0
    };

    private static string GetVariableLabel(string variable) => variable switch
    {
        "Temperature" or "SoilTemperature" => "Température (°C)",
        "Humidity" => "Humidité (%)",
        "Battery" => "Batterie (%)",
        "Permittivite" => "Permittivité",
        "MineralVWC" => "VWC Minérale (%)",
        "OrganicVWC" => "VWC Organique (%)",
        "PeatMixVWC" => "VWC Tourbe (%)",
        "CoirVWC" => "VWC Coco (%)",
        "MinWoolVWC" => "VWC Laine Minérale (%)",
        "PerliteVWC" => "VWC Perlite (%)",
        "MineralECp" => "EC Minérale (mS/cm)",
        "OrganicECp" => "EC Organique (mS/cm)",
        _ => variable
    };

    // Professional AgTech color palette - harmonious and readable
    private static readonly SKColor[] SeriesColors =
    [
        new SKColor(46, 125, 50),    // Forest Green - Primary
        new SKColor(2, 119, 189),    // Water Blue - Secondary  
        new SKColor(249, 168, 37),   // Sun Amber - Accent
        new SKColor(198, 40, 40),    // Alert Red
        new SKColor(142, 36, 170),   // Purple
        new SKColor(0, 137, 123),    // Teal
        new SKColor(255, 111, 0),    // Deep Orange
        new SKColor(63, 81, 181),    // Indigo
        new SKColor(233, 30, 99),    // Pink
        new SKColor(0, 150, 136),    // Cyan
        new SKColor(255, 193, 7),    // Yellow
        new SKColor(121, 85, 72),    // Brown
        new SKColor(96, 125, 139),   // Blue Grey
        new SKColor(156, 39, 176),   // Deep Purple
        new SKColor(76, 175, 80),    // Light Green
        new SKColor(255, 87, 34),    // Deep Orange Red
    ];

    private static SKColor GetSeriesColor(int index) => SeriesColors[index % SeriesColors.Length];

    private static SKColor GetVariableColor(string variable) => variable switch
    {
        "Temperature" or "SoilTemperature" => new SKColor(198, 40, 40),
        "Humidity" => new SKColor(2, 119, 189),
        "Battery" => new SKColor(46, 125, 50),
        "Permittivite" => new SKColor(249, 168, 37),
        "MineralVWC" => new SKColor(0, 137, 123),
        "OrganicVWC" => new SKColor(0, 172, 193),
        "PeatMixVWC" => new SKColor(38, 166, 154),
        "CoirVWC" => new SKColor(77, 182, 172),
        "MinWoolVWC" => new SKColor(128, 203, 196),
        "PerliteVWC" => new SKColor(178, 223, 219),
        "MineralECp" => new SKColor(142, 36, 170),
        "OrganicECp" => new SKColor(171, 71, 188),
        _ => new SKColor(120, 144, 156)
    };

    [RelayCommand]
    private void ToggleVariable(ChartVariableOption? option)
    {
        if (option is null) return;
        option.IsVisible = !option.IsVisible;
    }

    [RelayCommand]
    private void ChangeVariable(object? parameter)
    {
        var variable = parameter as string;
        if (string.IsNullOrWhiteSpace(variable))
        {
            return;
        }

        SelectedVariable = variable;
        _ = LoadChartDataAsync();
    }

    [ObservableProperty] private int _currentRangeDays = DefaultRangeDays;

    [RelayCommand]
    private void SetDateRange(object? parameter)
    {
        if (parameter is null)
        {
            return;
        }

        if (!int.TryParse(parameter.ToString(), out var days))
        {
            return;
        }

        CurrentRangeDays = days;
        ApplyDateRange(days);
        _ = LoadChartDataAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private void SetConfigVisibility(bool isVisible)
    {
        IsConfigVisible = isVisible;
    }

    [RelayCommand]
    private void PreviousPeriod()
    {
        var span = EndDate - StartDate;
        EndDate = StartDate;
        StartDate = EndDate - span;
        _ = LoadChartDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteNextPeriod))]
    private void NextPeriod()
    {
        var span = EndDate - StartDate;
        StartDate = EndDate;
        EndDate = StartDate + span;
        _ = LoadChartDataAsync();
    }

    private bool CanExecuteNextPeriod()
    {
        return EndDate < DateTime.Now.Date;
    }

    [RelayCommand]
    private async Task NavigateToConfigAsync()
    {
        if (string.IsNullOrEmpty(DevEui)) return;

        await _navigationService.NavigateToDeviceDetailAsync(DevEui);
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToMainAsync();
    }

    [RelayCommand]
    private async Task SaveConfigurationAsync()
    {
        if (Device is null)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            var update = new Kk.Kharts.Shared.DTOs.DeviceConfigUpdateDTO
            {
                Name = Name,
                Description = Description,
                InstallationLocation = InstallationLocation,
                ActiveInKropKontrol = IsActive
            };

            var result = await _apiService.UpdateDeviceConfigAsync(Device.DevEui, update);
            if (result.IsFailure)
            {
                SetError(result.Error ?? "Erreur lors de l'enregistrement");
                return;
            }

            await Shell.Current.DisplayAlertAsync("Succès", "Configuration enregistrée", "OK");
        }, "Erreur lors de l'enregistrement");
    }

    private void ResetDateRange()
    {
        CurrentRangeDays = DefaultRangeDays;
        ApplyDateRange(DefaultRangeDays, suppressUiUpdate: true);
    }

    private void ApplyDateRange(int days, bool suppressUiUpdate = false)
    {
        var now = DateTime.Now;
        EndDate = now;
        StartDate = now.AddDays(-days);

        if (!suppressUiUpdate)
        {
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
        }
    }
}
