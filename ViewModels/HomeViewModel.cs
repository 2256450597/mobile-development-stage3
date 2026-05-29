using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IAccelerometerService _accelerometer;
    private readonly IHapticService _haptic;

    // === Search ===
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilters();
        }
    }

    // === Category tabs ===
    public List<CategoryTab> Categories { get; } = new();

    private FoodCategory? _selectedCategory;
    public FoodCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
                ApplyFilters();
        }
    }

    // === Recipe grid (2-column waterfall) ===
    public ObservableCollection<Recipe> RecipeGrid { get; } = new();

    // === Today's Meals (horizontal row) ===
    public ObservableCollection<MealPlanEntry> TodayMeals { get; } = new();

    // === Curated collections ===
    public ObservableCollection<CuratedCollection> Collections { get; } = new();

    // === Commands ===
    public ICommand NavigateToRecipeDetailCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand ShowQuickMenuCommand { get; }

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

        SearchCommand = new Command(() =>
        {
            _haptic.PerformClick();
            ApplyFilters();
        });

        SelectCategoryCommand = new Command<FoodCategory?>(cat =>
        {
            _haptic.PerformClick();
            SelectedCategory = SelectedCategory == cat ? null : cat;
        });

        ShowQuickMenuCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            var action = await Shell.Current.DisplayActionSheet(
                "Quick Actions", "Cancel", null,
                "Take Photo", "Nearby Stores", "Surprise Me", "All Recipes");

            switch (action)
            {
                case "Take Photo":
                    await Shell.Current.GoToAsync("camera");
                    break;
                case "Nearby Stores":
                    await Shell.Current.GoToAsync("nearby");
                    break;
                case "Surprise Me":
                    var recipes = _dataService.GetAllRecipes();
                    var r = recipes[new Random().Next(recipes.Count)];
                    await Shell.Current.GoToAsync($"recipedetail?id={r.Id}");
                    break;
                case "All Recipes":
                    await Shell.Current.GoToAsync("//recipes");
                    break;
            }
        });

        _accelerometer.ShakeDetected += OnShakeDetected;
        _accelerometer.StartShakeDetection();

        LoadCategories();
        LoadTodayMeals();
        LoadCollections();
        ApplyFilters();
    }

    private async void OnShakeDetected(object? sender, EventArgs e)
    {
        _haptic.PerformLongPress();
        var recipes = _dataService.GetAllRecipes();
        var random = recipes[new Random().Next(recipes.Count)];
        await Shell.Current.GoToAsync($"recipedetail?id={random.Id}");
    }

    private void LoadCategories()
    {
        Categories.Clear();
        Categories.Add(new CategoryTab { Name = "All", Category = null, Selected = true });
        foreach (FoodCategory cat in Enum.GetValues(typeof(FoodCategory)))
            Categories.Add(new CategoryTab { Name = cat.ToString(), Category = cat, Selected = false });
    }

    private void LoadTodayMeals()
    {
        TodayMeals.Clear();
        var today = DateTime.Now.DayOfWeek;
        var monday = DateTime.Now.AddDays(-(int)today + 1);
        foreach (var entry in _dataService.GetMealPlanForWeek(monday).Where(e => e.Day == today))
            TodayMeals.Add(entry);
    }

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

    private void ApplyFilters()
    {
        RecipeGrid.Clear();
        List<Recipe> results;

        if (!string.IsNullOrWhiteSpace(SearchText))
            results = _dataService.SearchRecipes(SearchText);
        else if (SelectedCategory.HasValue)
            results = _dataService.GetRecipesByCategory(SelectedCategory.Value);
        else
            results = _dataService.GetAllRecipes();

        foreach (var recipe in results)
            RecipeGrid.Add(recipe);

        // Update category selection state
        foreach (var cat in Categories)
            cat.Selected = cat.Category == SelectedCategory;
    }

    public void Cleanup()
    {
        _accelerometer.StopShakeDetection();
        _accelerometer.ShakeDetected -= OnShakeDetected;
    }
}

public class CategoryTab : BaseViewModel
{
    public string Name { get; set; } = string.Empty;
    public FoodCategory? Category { get; set; }

    private bool _selected;
    public bool Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }
}

public class CuratedCollection
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<Recipe> Recipes { get; set; } = new();
}
