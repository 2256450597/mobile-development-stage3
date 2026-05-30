namespace TastyMealPlanner.Models;

/// <summary>Links a recipe to a specific day and meal type in the weekly meal plan.</summary>
public class MealPlanEntry
{
    /// <summary>Gets or sets the unique identifier for this meal plan entry. Auto-generated as a new GUID.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the day of the week this entry is scheduled for.</summary>
    public DayOfWeek Day { get; set; }

    /// <summary>Gets or sets which meal of the day (breakfast, lunch, or dinner) this entry represents.</summary>
    public MealType MealType { get; set; }

    /// <summary>Gets or sets the recipe assigned to this meal plan slot. Null if no recipe has been selected.</summary>
    public Recipe? Recipe { get; set; }
}
