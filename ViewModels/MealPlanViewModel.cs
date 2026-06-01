using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using TastyMealPlanner.Helpers;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class MealPlanViewModel : BaseViewModel
{
    private readonly IMealPlanService _mealPlan;
    private readonly IShoppingListService _shoppingList;
    private readonly IHapticService _haptic;
    private static readonly CultureInfo EnUs = new("en-US");

    // === Week navigation ===
    private DateTime _weekStart;
    public DateTime WeekStart
    {
        get => _weekStart;
        set
        {
            if (SetProperty(ref _weekStart, value))
            {
                OnPropertyChanged(nameof(WeekLabel));
                OnPropertyChanged(nameof(IsCurrentWeek));
                OnPropertyChanged(nameof(ShowBackToCurrent));
                OnPropertyChanged(nameof(TodayLabel));
                LoadDayStrip();
            }
        }
    }

    public string WeekLabel
    {
        get
        {
            var end = _weekStart.AddDays(6);
            return $"{_weekStart.ToString("MMM d", EnUs)} – {end.ToString("MMM d, yyyy", EnUs)}";
        }
    }

    public bool IsCurrentWeek
    {
        get
        {
            var today = DateTime.Now.Date;
            var currentMonday = today.AddDays(-(int)today.DayOfWeek + 1);
            return _weekStart.Date == currentMonday.Date;
        }
    }

    public bool ShowBackToCurrent => !IsCurrentWeek;

    public string TodayLabel
    {
        get
        {
            var today = DateTime.Now.Date;
            var currentMonday = today.AddDays(-(int)today.DayOfWeek + 1);
            if (_weekStart.Date < currentMonday.Date) return "Today →";
            if (_weekStart.Date > currentMonday.Date) return "← Today";
            return "Today";
        }
    }

    // === Day strip ===
    public ObservableCollection<DayStripItem> DayStrip { get; } = new();

    private DayOfWeek _selectedDay;
    public DayOfWeek SelectedDay
    {
        get => _selectedDay;
        set
        {
            if (SetProperty(ref _selectedDay, value))
            {
                LoadSelectedDayMeals();
                RefreshDayStripSelection();
            }
        }
    }

    public ObservableCollection<MealPlanEntry> SelectedDayMeals { get; } = new();

    // === Header for detail panel ===
    public string SelectedDayHeader
    {
        get
        {
            var date = _weekStart.AddDays((int)_selectedDay);
            var today = DateTime.Now.DayOfWeek == _selectedDay && IsCurrentWeek;
            var suffix = today ? " · Today" : "";
            return $"{date.ToString("dddd, MMMM d", EnUs)}{suffix}";
        }
    }

    public string MealCountLabel => SelectedDayMeals.Count == 1
        ? "1 meal" : $"{SelectedDayMeals.Count} meals";

    // === Commands ===
    public ICommand GoToPreviousWeekCommand { get; }
    public ICommand GoToNextWeekCommand { get; }
    public ICommand BackToCurrentWeekCommand { get; }
    public ICommand SelectDayCommand { get; }
    public ICommand RemoveMealCommand { get; }
    public ICommand NavigateToDetailCommand { get; }
    public ICommand AddMealCommand { get; }
    public ICommand RefreshCommand { get; }

    public MealPlanViewModel(IMealPlanService mealPlan, IShoppingListService shoppingList, IHapticService haptic)
    {
        _mealPlan = mealPlan;
        _shoppingList = shoppingList;
        _haptic = haptic;
        Title = "Meal Plan";

        var today = DateTime.Now.Date;
        _weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
        _selectedDay = today.DayOfWeek;

        GoToPreviousWeekCommand = new Command(() =>
        {
            _haptic.PerformClick();
            WeekStart = _weekStart.AddDays(-7);
        });

        GoToNextWeekCommand = new Command(() =>
        {
            _haptic.PerformClick();
            WeekStart = _weekStart.AddDays(7);
        });

        BackToCurrentWeekCommand = new Command(() =>
        {
            _haptic.PerformClick();
            var today2 = DateTime.Now.Date;
            WeekStart = today2.AddDays(-(int)today2.DayOfWeek + 1);
            SelectedDay = today2.DayOfWeek;
        });

        SelectDayCommand = new Command<DayStripItem>(item =>
        {
            if (item == null) return;
            _haptic.PerformClick();
            SelectedDay = item.Day;
        });

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
                _mealPlan.RemoveFromMealPlan(entry.Id);
                RegenerateShoppingList();
                LoadSelectedDayMeals();
                LoadDayStrip();
            }
        });

        AddMealCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            RecipesViewModel.PendingSelectionDay = SelectedDay;
            await Shell.Current.GoToAsync("//recipes");
        });

        RefreshCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            Reload();
            await Task.Delay(400);
            IsBusy = false;
        });

        LoadDayStrip();
        LoadSelectedDayMeals();
    }

    public void Reload()
    {
        LoadDayStrip();
        LoadSelectedDayMeals();
    }

    private void LoadDayStrip()
    {
        DayStrip.Clear();
        var today = DateTime.Now.Date;
        for (int i = 0; i < 7; i++)
        {
            var date = _weekStart.AddDays(i);
            var day = date.DayOfWeek;
            var meals = _mealPlan.GetMealPlanForWeek(_weekStart)
                .Count(e => e.Day == day);

            DayStrip.Add(new DayStripItem
            {
                Day = day,
                DayAbbr = day.ToString().Substring(0, 3),
                DateNumber = date.Day,
                IsToday = date.Date == today,
                IsSelected = day == _selectedDay,
                MealCount = meals
            });
        }
    }

    private void RefreshDayStripSelection()
    {
        foreach (var item in DayStrip)
            item.IsSelected = item.Day == _selectedDay;

        OnPropertyChanged(nameof(SelectedDayHeader));
        OnPropertyChanged(nameof(MealCountLabel));
    }

    private void RegenerateShoppingList()
    {
        _shoppingList.RemoveAutoGeneratedItems();

        var today = DateTime.Now.DayOfWeek;
        var todayMeals = _mealPlan.GetMealPlanForWeek(_weekStart)
            .Where(e => e.Day == today && e.Recipe != null);

        foreach (var meal in todayMeals)
        {
            foreach (var ingredient in meal.Recipe!.Ingredients)
            {
                var (qty, name) = IngredientParser.Parse(ingredient);

                _shoppingList.AddShoppingItem(new ShoppingItem
                {
                    Name = string.IsNullOrEmpty(name) ? "" : char.ToUpper(name[0]) + name[1..],
                    Quantity = qty,
                    IsAutoGenerated = true
                });
            }
        }
    }

    private void LoadSelectedDayMeals()
    {
        SelectedDayMeals.Clear();
        var meals = _mealPlan.GetMealPlanForWeek(_weekStart)
            .Where(e => e.Day == _selectedDay)
            .OrderBy(e => e.MealType)
            .ToList();

        foreach (var m in meals)
            SelectedDayMeals.Add(m);

        OnPropertyChanged(nameof(MealCountLabel));
        OnPropertyChanged(nameof(SelectedDayHeader));
    }
}

public class DayStripItem
{
    public DayOfWeek Day { get; set; }
    public string DayAbbr { get; set; } = string.Empty;
    public int DateNumber { get; set; }
    public bool IsToday { get; set; }
    public bool IsSelected { get; set; }
    public int MealCount { get; set; }
    public bool HasMeals => MealCount > 0;
    public string MealCountText => MealCount == 0 ? "" : MealCount.ToString();
}
