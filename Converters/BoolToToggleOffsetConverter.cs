using System.Globalization;

namespace TastyMealPlanner.Converters;

/// <summary>Converts bool to horizontal offset for toggle switch thumb: true→right, false→left.</summary>
public class BoolToToggleOffsetConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? 22.0 : 0.0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
