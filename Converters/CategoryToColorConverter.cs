using System.Globalization;
using TastyMealPlanner.Models;

namespace TastyMealPlanner.Converters;

/// <summary>Maps a FoodCategory enum value to its associated display colour.</summary>
public class CategoryToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FoodCategory category)
        {
            return category switch
            {
                FoodCategory.Breakfast => Color.FromArgb("#FF9800"),
                FoodCategory.Lunch => Color.FromArgb("#4CAF50"),
                FoodCategory.Dinner => Color.FromArgb("#E91E63"),
                FoodCategory.Dessert => Color.FromArgb("#9C27B0"),
                FoodCategory.Snack => Color.FromArgb("#FF5722"),
                FoodCategory.Drink => Color.FromArgb("#2196F3"),
                _ => Color.FromArgb("#757575")
            };
        }
        return Color.FromArgb("#757575");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
