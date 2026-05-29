using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class MealPlanViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IHapticService _haptic;

    public ObservableCollection<DayPlanGroup> WeekPlan { get; } = new();

    public ICommand RemoveMealCommand { get; }
    public ICommand NavigateToDetailCommand { get; }
    public ICommand AddMealCommand { get; }

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

public class DayPlanGroup
{
    public DayOfWeek Day { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int Index { get; set; }
    public ObservableCollection<MealPlanEntry> Entries { get; set; } = new();
    public bool IsEven => Index % 2 == 0;
}
