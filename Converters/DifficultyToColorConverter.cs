using System.Globalization;

namespace TastyMealPlanner.Converters;

/// <summary>Maps a difficulty string (Easy/Medium/Hard) to a semantic colour.</summary>
public class DifficultyToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string difficulty)
        {
            return difficulty switch
            {
                "Easy" => Color.FromArgb("#4CAF50"),
                "Medium" => Color.FromArgb("#FF9800"),
                "Hard" => Color.FromArgb("#F44336"),
                _ => Color.FromArgb("#757575")
            };
        }
        return Color.FromArgb("#757575");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
