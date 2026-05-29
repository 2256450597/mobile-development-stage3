using System.Globalization;

namespace TastyMealPlanner.Converters;

/// <summary>Converts a boolean (checked state) to TextDecorations.Strikethrough for shopping list items.</summary>
public class BoolToStrikethroughConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
            return TextDecorations.Strikethrough;
        return TextDecorations.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
