using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Main discovery/home page. Shows today's meals, curated collections, and full recipe feed.</summary>
public class HomeViewModel : BaseViewModel
{
    private readonly IRecipeService _recipes;
    private readonly IMealPlanService _mealPlan;
    private readonly IAccelerometerService _accelerometer;
    private readonly IHapticService _haptic;

    // === Recipe grid (2-column waterfall) ===
    public ObservableCollection<Recipe> RecipeGrid { get; } = new();
    public ObservableCollection<Recipe> FavoriteRecipes { get; } = new();

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
    public ICommand OpenCameraCommand { get; }
    public ICommand OpenNearbyCommand { get; }
    public ICommand RandomRecipeCommand { get; }
    public ICommand OpenRecipesCommand { get; }
    public ICommand AddTodayMealCommand { get; }
    public ICommand RemoveTodayMealCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand DismissShakeResultCommand { get; }
    public ICommand ViewShakeResultCommand { get; }

    public HomeViewModel(IRecipeService recipes,
                         IMealPlanService mealPlan,
                         IAccelerometerService accelerometer,
                         IHapticService haptic)
    {
        _recipes = recipes;
        _mealPlan = mealPlan;
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
                    var recipes = _recipes.GetAllRecipes();
                    if (recipes.Count == 0) return;
                    var r = recipes[new Random().Next(recipes.Count)];
                    await Shell.Current.GoToAsync($"recipedetail?id={r.Id}");
                    break;
                case "☰  All Recipes":
                    await Shell.Current.GoToAsync("//recipes");
                    break;
            }
        });

        OpenCameraCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("camera");
        });

        OpenNearbyCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("nearby");
        });

        RandomRecipeCommand = new Command(() =>
        {
            _haptic.PerformClick();
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450)); }
            catch { /* Non-critical: vibration unavailable on this device */ }
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
            catch { /* Non-critical: haptic feedback unavailable */ }
            var recipes = _recipes.GetAllRecipes();
            if (recipes.Count == 0) return;
            ShakeResult = recipes[new Random().Next(recipes.Count)];
        });

        OpenRecipesCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("//recipes");
        });

        AddTodayMealCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            RecipesViewModel.PendingSelectionDay = DateTime.Now.DayOfWeek;
            await Shell.Current.GoToAsync("//recipes");
        });

        RemoveTodayMealCommand = new Command<MealPlanEntry>(entry =>
        {
            _haptic.PerformClick();
            _mealPlan.RemoveFromMealPlan(entry.Id);
            TodayMeals.Remove(entry);
        });

        RefreshCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            Reload();
            await Task.Delay(400);
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
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450)); }
            catch { /* Non-critical: vibration unavailable */ }
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
            catch { /* Non-critical: haptic feedback unavailable */ }
            var id = ShakeResult.Id;
            ShakeResult = null;
            await Shell.Current.GoToAsync($"recipedetail?id={id}");
        });

        _accelerometer.ShakeDetected += OnShakeDetected;

        LoadTodayMeals();
        LoadCollections();
        LoadAllRecipes();
    }

    private void OnShakeDetected(object? sender, EventArgs e)
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450)); }
        catch { /* Non-critical: vibration unavailable */ }
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
        catch { /* Non-critical: haptic feedback unavailable */ }

        var recipes = _recipes.GetAllRecipes();
        if (recipes.Count == 0) return;
        ShakeResult = recipes[new Random().Next(recipes.Count)];
    }

    /// <summary>Loads meals planned for the current day of the week.</summary>
    private void LoadTodayMeals()
    {
        TodayMeals.Clear();
        var today = DateTime.Now.DayOfWeek;
        var monday = DateTime.Now.AddDays(-(int)today + 1);
        foreach (var entry in _mealPlan.GetMealPlanForWeek(monday).Where(e => e.Day == today))
            TodayMeals.Add(entry);

        // Always show the + Add Meal placeholder so the user can plan meals for today
        TodayMeals.Add(new MealPlanEntry { Recipe = null, Day = today });
    }

    /// <summary>Builds curated recipe collections: Quick and Easy, Weekend Indulgence, High Protein.</summary>
    private void LoadCollections()
    {
        Collections.Clear();
        var all = _recipes.GetAllRecipes();
        Collections.Add(new CuratedCollection
        {
            Title = "Quick & Easy",
            Subtitle = "Ready in 15 min or less",
            Recipes = all.Where(r => r.PrepTimeMinutes + r.CookTimeMinutes <= 15).Take(8).ToList()
        });
        Collections.Add(new CuratedCollection
        {
            Title = "Weekend Indulgence",
            Subtitle = "Baked treats & fresh drinks",
            Recipes = all.Where(r => r.Category is FoodCategory.Baked
                || (r.Category is FoodCategory.Fresh && (r.Name.Contains("Lemonade") || r.Name.Contains("Lassi") || r.Name.Contains("Smoothie")))).Take(8).ToList()
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
        foreach (var recipe in _recipes.GetAllRecipes())
            RecipeGrid.Add(recipe);
    }

    /// <summary>Starts accelerometer-based shake detection. Call from OnAppearing when page is fully loaded.</summary>
    public void StartAccelerometer()
    {
        try { _accelerometer.StartShakeDetection(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Shake unavailable: {ex.Message}"); }
    }

    /// <summary>Stops shake detection and unsubscribes from sensor events. Call when navigating away.</summary>
    public void Reload()
    {
        LoadTodayMeals();
        LoadCollections();
        LoadAllRecipes();
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        FavoriteRecipes.Clear();
        foreach (var r in _recipes.GetAllRecipes().Where(r => r.IsFavorite))
            FavoriteRecipes.Add(r);
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
