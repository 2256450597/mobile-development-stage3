namespace TastyMealPlanner.Models;

/// <summary>
/// Represents a complete recipe with ingredients, instructions, and nutritional information.
/// </summary>
public class Recipe
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public FoodCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public List<string> Instructions { get; set; } = new();
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Calories { get; set; }
    public int Servings { get; set; }
    public string Difficulty { get; set; } = "Easy";
    public bool IsFavorite { get; set; }
}
