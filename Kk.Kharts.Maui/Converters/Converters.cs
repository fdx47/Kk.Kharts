using System.Globalization;

namespace Kk.Kharts.Maui.Converters;

/// <summary>
/// Converts a boolean to view mode text (Grouped/List).
/// </summary>
public class BoolToViewModeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isGrouped)
            return isGrouped ? "📋 Liste" : "🏢 Grouper";
        return "📋 Liste";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean into a rotation angle (0° expanded, -90° collapsed).
/// </summary>
public class BoolToRotationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isExpanded && isExpanded ? 0d : -90d;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts zero to true (for empty state visibility).
/// </summary>
public class ZeroToTrueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
            return count == 0;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
