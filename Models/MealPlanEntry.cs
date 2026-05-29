namespace TastyMealPlanner.Models;

/// <summary>Links a recipe to a specific day and meal type in the weekly meal plan.</summary>
public class MealPlanEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DayOfWeek Day { get; set; }
    public MealType MealType { get; set; }
    public Recipe? Recipe { get; set; }
}
