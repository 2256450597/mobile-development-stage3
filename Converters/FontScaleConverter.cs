using System.Globalization;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.Converters;

public class FontScaleConverter : IValueConverter
{
    public double SmallFontScale { get; set; } = 0.85;

    public double MediumFontScale { get; set; } = 1.0;

    public double LargeFontScale { get; set; } = 1.3;

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

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    private static ThemeService? GetThemeService()
    {
        if (Application.Current?.Handler?.MauiContext?.Services != null)
        {
            return Application.Current.Handler.MauiContext.Services.GetService<ThemeService>();
        }
        return null;
    }
}
