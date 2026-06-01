using TastyMealPlanner.Models;

namespace TastyMealPlanner.Services;

/// <summary>In-memory meal plan store supporting week-based queries and entry management.</summary>
public class MealPlanService : IMealPlanService
{
    private readonly List<MealPlanEntry> _entries = new();

    public List<MealPlanEntry> GetMealPlanForWeek(DateTime weekStart)
    {
        var weekDays = new HashSet<DayOfWeek>();
        for (int i = 0; i < 7; i++)
            weekDays.Add(weekStart.AddDays(i).DayOfWeek);

        return _entries.Where(m => weekDays.Contains(m.Day)).ToList();
    }

    public void AddToMealPlan(MealPlanEntry entry) => _entries.Add(entry);

    public void RemoveFromMealPlan(string entryId)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry != null)
            _entries.Remove(entry);
    }
}
