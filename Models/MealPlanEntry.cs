namespace TastyMealPlanner.Models;

public class MealPlanEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DayOfWeek Day { get; set; }

    public MealType MealType { get; set; }

    public Recipe? Recipe { get; set; }
}
