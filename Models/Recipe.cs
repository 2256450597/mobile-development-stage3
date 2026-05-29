namespace TastyMealPlanner.Models;

/// <summary>
/// Represents a complete recipe with ingredients, instructions, and nutritional information.
/// </summary>
/// <summary>Represents a complete recipe with ingredients, instructions, nutritional info, and an image.</summary>
public class Recipe
{
    /// <summary>Unique identifier for the recipe.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>Display name of the recipe.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Food category (Breakfast, Lunch, Dinner, Dessert, Snack, Drink).</summary>
    public FoodCategory Category { get; set; }
    /// <summary>Short description of the dish.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>File path or URL of the recipe image.</summary>
    public string ImageUrl { get; set; } = string.Empty;
    /// <summary>List of ingredient strings (e.g. "2 cups flour").</summary>
    public List<string> Ingredients { get; set; } = new();
    /// <summary>Ordered list of preparation steps.</summary>
    public List<string> Instructions { get; set; } = new();
    /// <summary>Preparation time in minutes.</summary>
    public int PrepTimeMinutes { get; set; }
    /// <summary>Cooking time in minutes.</summary>
    public int CookTimeMinutes { get; set; }
    /// <summary>Calorie count per serving.</summary>
    public int Calories { get; set; }
    /// <summary>Number of servings the recipe yields.</summary>
    public int Servings { get; set; }
    /// <summary>Difficulty level (Easy, Medium, Hard).</summary>
    public string Difficulty { get; set; } = "Easy";
    /// <summary>Whether the recipe is marked as a favourite by the user.</summary>
    public bool IsFavorite { get; set; }
}
