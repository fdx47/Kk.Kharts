using System.Globalization;
using Kk.Kharts.Maui.Models;
using Microsoft.Maui.Controls;

namespace Kk.Kharts.Maui.Converters;

public class SetColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is ChartVariableOption option && values[1] is Color color)
        {
            return new object[] { option, color };
        }
        return Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
