using System.Globalization;
using System.Windows.Data;

namespace BreakdownManager.App.Converters;

/// <summary>Compares a bound string to the converter parameter, used to drive nav-tab RadioButtons.</summary>
public class StringEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
