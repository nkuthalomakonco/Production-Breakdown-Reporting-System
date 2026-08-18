using System.Globalization;
using System.Windows.Data;

namespace BreakdownManager.App.Converters;

/// <summary>Lets a group of RadioButtons bind directly to a single enum property.</summary>
public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is not null)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }
        return Binding.DoNothing;
    }
}
