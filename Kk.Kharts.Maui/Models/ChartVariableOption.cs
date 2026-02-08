using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using SkiaSharp;

namespace Kk.Kharts.Maui.Models;

public sealed class ChartVariableOption : INotifyPropertyChanged
{
    private bool _isVisible;
    private bool _isColorPaletteVisible;
    private double? _min;
    private double? _max;
    private Color _accentColor;
    private SKColor _accentSkColor;

    public string Key { get; }
    public string Label { get; }

    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            if (_accentColor == value) return;
            _accentColor = value;
            OnPropertyChanged();
        }
    }

    public SKColor AccentSkColor
    {
        get => _accentSkColor;
        set
        {
            if (_accentSkColor == value) return;
            _accentSkColor = value;
            OnPropertyChanged();
        }
    }

    public double? Min
    {
        get => _min;
        set
        {
            if (_min == value) return;
            _min = value;
            OnPropertyChanged();
        }
    }

    public double? Max
    {
        get => _max;
        set
        {
            if (_max == value) return;
            _max = value;
            OnPropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value) return;
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsColorPaletteVisible
    {
        get => _isColorPaletteVisible;
        set
        {
            if (_isColorPaletteVisible == value) return;
            _isColorPaletteVisible = value;
            OnPropertyChanged();
        }
    }

    public ChartVariableOption(string key, string label, Color accentColor, SKColor accentSkColor, bool isVisible = true)
    {
        Key = key;
        Label = label;
        _accentColor = accentColor;
        _accentSkColor = accentSkColor;
        _isVisible = isVisible;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
