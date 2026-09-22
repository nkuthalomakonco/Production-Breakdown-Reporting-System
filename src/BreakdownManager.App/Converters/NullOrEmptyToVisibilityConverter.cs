using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BreakdownManager.App.Converters;

/// <summary>Collapses an element when the bound string is null or empty — used for the login error message.</summary>
public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
