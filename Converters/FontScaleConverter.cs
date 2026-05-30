using System.Globalization;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.Converters;

/// <summary>XAML value converter that scales a base font size by the ThemeService's current FontScale factor.</summary>
public class FontScaleConverter : IValueConverter
{
    /// <summary>Gets or sets the scale factor applied when the font size option is Small. Default is 0.85.</summary>
    public double SmallFontScale { get; set; } = 0.85;

    /// <summary>Gets or sets the scale factor applied when the font size option is Medium. Default is 1.0.</summary>
    public double MediumFontScale { get; set; } = 1.0;

    /// <summary>Gets or sets the scale factor applied when the font size option is Large. Default is 1.3.</summary>
    public double LargeFontScale { get; set; } = 1.3;

    /// <summary>Multiplies a numeric base font size by the current FontScale from ThemeService.</summary>
    /// <param name="value">The base font size to scale. Should be a double.</param>
    /// <param name="targetType">The target binding type (unused).</param>
    /// <param name="parameter">An optional converter parameter (unused).</param>
    /// <param name="culture">The culture info for conversion (unused).</param>
    /// <returns>The scaled font size, or the original value if it is not a double.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double baseSize)
        {
            var themeService = GetThemeService();
            if (themeService == null) return baseSize;
            return baseSize * themeService.FontScale;
        }
        return value;
    }

    /// <summary>Not implemented. ConvertBack is not supported by this converter.</summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    /// <summary>Resolves the ThemeService instance from the MAUI dependency injection container.</summary>
    /// <returns>The ThemeService singleton, or null if the service provider is not available.</returns>
    private static ThemeService? GetThemeService()
    {
        if (Application.Current?.Handler?.MauiContext?.Services != null)
        {
            return Application.Current.Handler.MauiContext.Services.GetService<ThemeService>();
        }
        return null;
    }
}
