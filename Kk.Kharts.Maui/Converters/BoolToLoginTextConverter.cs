using System.Globalization;

namespace Kk.Kharts.Maui.Converters;

public class BoolToLoginTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isBusy)
            return isBusy ? "Connexion..." : "Se connecter";
        return "Se connecter";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
