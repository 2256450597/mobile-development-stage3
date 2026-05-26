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

    public ObservableCollection<MealPlanEntry> TodayMeals { get; } = new();
    public ObservableCollection<Recipe> UpcomingRecipes { get; } = new();

    public ICommand NavigateToCameraCommand { get; }
    public ICommand NavigateToNearbyCommand { get; }
    public ICommand SurpriseMeCommand { get; }
    public ICommand NavigateToRecipeDetailCommand { get; }
    public ICommand NavigateToRecipesCommand { get; }

    public HomeViewModel(IDataService dataService,
                         IAccelerometerService accelerometer,
                         IHapticService haptic)
    {
        _dataService = dataService;
        _accelerometer = accelerometer;
        _haptic = haptic;
        Title = "Tasty Meal Planner";

        NavigateToCameraCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("camera");
        });

        NavigateToNearbyCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("nearby");
        });

        SurpriseMeCommand = new Command(async () =>
        {
            _haptic.PerformLongPress();
            var recipes = _dataService.GetAllRecipes();
            var random = recipes[new Random().Next(recipes.Count)];
            await Shell.Current.GoToAsync($"recipedetail?id={random.Id}");
        });

        NavigateToRecipeDetailCommand = new Command<Recipe>(async (recipe) =>
        {
            _haptic.PerformClick();
            if (recipe != null)
                await Shell.Current.GoToAsync($"recipedetail?id={recipe.Id}");
        });

        NavigateToRecipesCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("//recipes");
        });

        // Shake to get random recipe
        _accelerometer.ShakeDetected += OnShakeDetected;
        _accelerometer.StartShakeDetection();

        LoadTodayMeals();
        LoadUpcoming();
    }

    private async void OnShakeDetected(object? sender, EventArgs e)
    {
        _haptic.PerformLongPress();
        var recipes = _dataService.GetAllRecipes();
        var random = recipes[new Random().Next(recipes.Count)];
        await Shell.Current.GoToAsync($"recipedetail?id={random.Id}");
    }

    public void Cleanup()
    {
        _accelerometer.StopShakeDetection();
        _accelerometer.ShakeDetected -= OnShakeDetected;
    }

    private void LoadTodayMeals()
    {
        TodayMeals.Clear();
        var today = DateTime.Now.DayOfWeek;
        var weekStart = DateTime.Now.AddDays(-(int)today + 1);
        var plan = _dataService.GetMealPlanForWeek(weekStart);

        foreach (var entry in plan.Where(e => e.Day == today))
            TodayMeals.Add(entry);
    }

    private void LoadUpcoming()
    {
        UpcomingRecipes.Clear();
        var allRecipes = _dataService.GetAllRecipes();
        foreach (var recipe in allRecipes.OrderBy(r => Guid.NewGuid()).Take(5))
            UpcomingRecipes.Add(recipe);
    }
}
