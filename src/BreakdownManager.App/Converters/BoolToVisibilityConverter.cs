using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BreakdownManager.App.Converters;

/// <summary>Shows an element when the bound bool is true, collapses it otherwise — used to gate the nav bar by login state/role.</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
