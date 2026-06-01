using System.Text.RegularExpressions;

namespace TastyMealPlanner.Helpers;

/// <summary>Parses ingredient strings like "2 cups flour" into (quantity, name) pairs.
/// Only treats the first word as quantity if it's numeric.</summary>
public static partial class IngredientParser
{
    /// <summary>Matches integer, decimal (1.5), fractions (1/2, 1/4), and unicode fractions (½, ¼).</summary>
    [GeneratedRegex(@"^[\d]+(\.[\d]+)?$|^[\d]+/[\d]+$|^[½⅓⅔¼¾⅕⅖⅗⅘⅙⅚⅛⅜⅝⅞]$")]
    private static partial Regex NumericPattern();

    /// <summary>Splits "2 cups flour" into ("2", "cups flour") or "Salt" into ("", "Salt").</summary>
    public static (string Quantity, string Name) Parse(string ingredient)
    {
        if (string.IsNullOrWhiteSpace(ingredient))
            return ("", "");

        var parts = ingredient.Split(' ', 2);
        if (parts.Length == 2 && NumericPattern().IsMatch(parts[0]))
        {
            return (parts[0], parts[1]);
        }

        return ("", ingredient);
    }
}
