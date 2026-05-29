using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Weekly meal planner showing 7 days with breakfast/lunch/dinner slots per day.</summary>
public class MealPlanViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IHapticService _haptic;

    /// <summary>Gets the collection of day-plan groups representing the full weekly meal plan.</summary>
    public ObservableCollection<DayPlanGroup> WeekPlan { get; } = new();

    /// <summary>Command to remove a meal plan entry after user confirmation.</summary>
    public ICommand RemoveMealCommand { get; }
    /// <summary>Command to navigate to the recipe detail page for a meal plan entry.</summary>
    public ICommand NavigateToDetailCommand { get; }
    /// <summary>Command to add a meal to a specific day by navigating to the recipes page.</summary>
    public ICommand AddMealCommand { get; }

    /// <summary>Initialises a new instance of the <see cref="MealPlanViewModel"/> class with data and haptic services, then loads the week plan.</summary>
    public MealPlanViewModel(IDataService dataService, IHapticService haptic)
    {
        _dataService = dataService;
        _haptic = haptic;
        Title = "Meal Plan";

        NavigateToDetailCommand = new Command<MealPlanEntry>(async (entry) =>
        {
            if (entry?.Recipe == null) return;
            _haptic.PerformClick();
            await Shell.Current.GoToAsync($"recipedetail?id={entry.Recipe.Id}");
        });

        RemoveMealCommand = new Command<MealPlanEntry>(async (entry) =>
        {
            if (entry == null) return;
            _haptic.PerformLongPress();
            bool confirm = await Shell.Current.DisplayAlert(
                "Remove Meal", $"Remove {entry.Recipe?.Name} from {entry.MealType}?", "Yes", "No");
            if (confirm)
            {
                _dataService.RemoveFromMealPlan(entry.Id);
                LoadWeekPlan();
            }
        });

        AddMealCommand = new Command<DayPlanGroup>(async (dayGroup) =>
        {
            _haptic.PerformClick();
            var day = dayGroup?.Day ?? DateTime.Now.DayOfWeek;
            // Signal RecipesPage to enter meal-plan selection mode
            RecipesViewModel.PendingSelectionDay = day;
            await Shell.Current.GoToAsync("//recipes");
        });

        LoadWeekPlan();
    }

    /// <summary>Rebuilds the full week plan from the data service. Called on appearing and after modifications.</summary>
    public void LoadWeekPlan()
    {
        WeekPlan.Clear();
        var monday = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + 1);
        var index = 0;

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var entries = _dataService.GetMealPlanForWeek(monday)
                .Where(e => e.Day == day)
                .OrderBy(e => e.MealType)
                .ToList();

            WeekPlan.Add(new DayPlanGroup
            {
                Day = day,
                DayName = day.ToString().Substring(0, 3),
                Index = index++,
                Entries = new ObservableCollection<MealPlanEntry>(entries)
            });
        }
    }
}

/// <summary>Groups meal plan entries by day of the week for display in the collection view.</summary>
public class DayPlanGroup
{
    /// <summary>Gets or sets the day of the week for this group.</summary>
    public DayOfWeek Day { get; set; }
    /// <summary>Gets or sets the abbreviated display name of the day (e.g. "Mon", "Tue").</summary>
    public string DayName { get; set; } = string.Empty;
    /// <summary>Gets or sets the zero-based index of this day in the week.</summary>
    public int Index { get; set; }
    /// <summary>Gets or sets the collection of meal plan entries assigned to this day.</summary>
    public ObservableCollection<MealPlanEntry> Entries { get; set; } = new();
    /// <summary>Gets whether this day has an even index, used for alternating row styling.</summary>
    public bool IsEven => Index % 2 == 0;
}
