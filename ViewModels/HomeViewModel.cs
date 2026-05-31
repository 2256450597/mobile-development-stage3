using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Main discovery/home page. Shows today's meals, curated collections, and full recipe feed.</summary>
public class HomeViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IAccelerometerService _accelerometer;
    private readonly IHapticService _haptic;

    // === Recipe grid (2-column waterfall) ===
    public ObservableCollection<Recipe> RecipeGrid { get; } = new();

    // === Today's Meals (horizontal row) ===
    public ObservableCollection<MealPlanEntry> TodayMeals { get; } = new();

    // === Curated collections ===
    public ObservableCollection<CuratedCollection> Collections { get; } = new();

    // === Shake result overlay ===
    private Recipe? _shakeResult;
    public Recipe? ShakeResult
    {
        get => _shakeResult;
        set
        {
            if (SetProperty(ref _shakeResult, value))
                OnPropertyChanged(nameof(HasShakeResult));
        }
    }
    public bool HasShakeResult => ShakeResult != null;

    // === Commands ===
    public ICommand NavigateToRecipeDetailCommand { get; }
    public ICommand ShowQuickMenuCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand DismissShakeResultCommand { get; }
    public ICommand ViewShakeResultCommand { get; }

    public HomeViewModel(IDataService dataService,
                         IAccelerometerService accelerometer,
                         IHapticService haptic)
    {
        _dataService = dataService;
        _accelerometer = accelerometer;
        _haptic = haptic;
        Title = "Home";

        NavigateToRecipeDetailCommand = new Command<Recipe>(async (recipe) =>
        {
            _haptic.PerformClick();
            if (recipe != null)
                await Shell.Current.GoToAsync($"recipedetail?id={recipe.Id}");
        });

        ShowQuickMenuCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            var action = await Shell.Current.DisplayActionSheet(
                "Quick Actions", "Cancel", null,
                "⌖  Take Photo", "⌂  Nearby Stores", "★  Surprise Me", "☰  All Recipes");

            switch (action)
            {
                case "⌖  Take Photo":
                    await Shell.Current.GoToAsync("camera");
                    break;
                case "⌂  Nearby Stores":
                    await Shell.Current.GoToAsync("nearby");
                    break;
                case "★  Surprise Me":
                    var recipes = _dataService.GetAllRecipes();
                    var r = recipes[new Random().Next(recipes.Count)];
                    await Shell.Current.GoToAsync($"recipedetail?id={r.Id}");
                    break;
                case "☰  All Recipes":
                    await Shell.Current.GoToAsync("//recipes");
                    break;
            }
        });

        RefreshCommand = new Command(() =>
        {
            _haptic.PerformClick();
            Reload();
            IsBusy = false;
        });

        DismissShakeResultCommand = new Command(() =>
        {
            _haptic.PerformClick();
            ShakeResult = null;
        });

        ViewShakeResultCommand = new Command(async () =>
        {
            if (ShakeResult == null) return;
            // NutriBite-style: vibration/haptic on button click runs on UI thread
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450)); } catch { }
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            var id = ShakeResult.Id;
            ShakeResult = null;
            await Shell.Current.GoToAsync($"recipedetail?id={id}");
        });

        _accelerometer.ShakeDetected += OnShakeDetected;
        _accelerometer.StartShakeDetection();

        LoadTodayMeals();
        LoadCollections();
        LoadAllRecipes();
    }

    private void OnShakeDetected(object? sender, EventArgs e)
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450)); } catch { }
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }

        var recipes = _dataService.GetAllRecipes();
        ShakeResult = recipes[new Random().Next(recipes.Count)];
    }

    /// <summary>Loads meals planned for the current day of the week.</summary>
    private void LoadTodayMeals()
    {
        TodayMeals.Clear();
        var today = DateTime.Now.DayOfWeek;
        var monday = DateTime.Now.AddDays(-(int)today + 1);
        foreach (var entry in _dataService.GetMealPlanForWeek(monday).Where(e => e.Day == today))
            TodayMeals.Add(entry);
    }

    /// <summary>Builds curated recipe collections: Quick and Easy, Weekend Indulgence, High Protein.</summary>
    private void LoadCollections()
    {
        Collections.Clear();
        var all = _dataService.GetAllRecipes();
        Collections.Add(new CuratedCollection
        {
            Title = "Quick & Easy",
            Subtitle = "Ready in 15 min or less",
            Recipes = all.Where(r => r.PrepTimeMinutes + r.CookTimeMinutes <= 15).Take(8).ToList()
        });
        Collections.Add(new CuratedCollection
        {
            Title = "Weekend Indulgence",
            Subtitle = "Desserts & drinks",
            Recipes = all.Where(r => r.Category is FoodCategory.Dessert or FoodCategory.Drink).Take(8).ToList()
        });
        Collections.Add(new CuratedCollection
        {
            Title = "High Protein",
            Subtitle = "Fuel your day",
            Recipes = all.Where(r => r.Calories >= 400).Take(8).ToList()
        });
    }

    /// <summary>Loads all recipes unfiltered into the recipe grid.</summary>
    private void LoadAllRecipes()
    {
        RecipeGrid.Clear();
        foreach (var recipe in _dataService.GetAllRecipes())
            RecipeGrid.Add(recipe);
    }

    /// <summary>Stops shake detection and unsubscribes from sensor events. Call when navigating away.</summary>
    public void Reload()
    {
        LoadTodayMeals();
        LoadCollections();
        LoadAllRecipes();
    }

    public void Cleanup()
    {
        _accelerometer.StopShakeDetection();
        _accelerometer.ShakeDetected -= OnShakeDetected;
    }
}

/// <summary>Represents a curated horizontal-scrolling collection of recipes (e.g. Quick and Easy).</summary>
public class CuratedCollection
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<Recipe> Recipes { get; set; } = new();
}
