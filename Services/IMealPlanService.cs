using TastyMealPlanner.Models;

namespace TastyMealPlanner.Services;

/// <summary>Meal plan management — week-based planning, add/remove entries.</summary>
public interface IMealPlanService
{
    List<MealPlanEntry> GetMealPlanForWeek(DateTime weekStart);
    void AddToMealPlan(MealPlanEntry entry);
    void RemoveFromMealPlan(string entryId);
}
