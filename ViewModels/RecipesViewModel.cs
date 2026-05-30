using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Searchable/filterable recipe browser with category tabs and real-time filtering.</summary>
public class RecipesViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IHapticService _haptic;

    public ObservableCollection<Recipe> Recipes { get; } = new();
    public ObservableCollection<FoodCategory> Categories { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterRecipes();
        }
    }

    private FoodCategory? _selectedCategory;
    public FoodCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);
            FilterRecipes();
        }
    }

    // Meal plan selection mode — set via static method before navigating from MealPlan
    public static DayOfWeek? PendingSelectionDay;

    private string _mealPlanDay = string.Empty;
    public string MealPlanDay
    {
        get => _mealPlanDay;
        set => SetProperty(ref _mealPlanDay, value);
    }

    public void EnterSelectionMode(DayOfWeek day)
    {
        var dayStr = day.ToString();
        MealPlanDay = dayStr;
        SelectedDay = day;
        IsSelectionMode = true;
        Title = $"Pick for {dayStr}";
    }

    public void ExitSelectionMode()
    {
        IsSelectionMode = false;
        MealPlanDay = string.Empty;
        Title = "Recipes";
    }

    private bool _isSelectionMode;
    public bool IsSelectionMode
    {
        get => _isSelectionMode;
        set => SetProperty(ref _isSelectionMode, value);
    }

    private DayOfWeek _selectedDay;
    public DayOfWeek SelectedDay
    {
        get => _selectedDay;
        set => SetProperty(ref _selectedDay, value);
    }

    public ICommand SelectCategoryCommand { get; }
    public ICommand ClearCategoryCommand { get; }
    public ICommand NavigateToRecipeCommand { get; }

    public RecipesViewModel(IDataService dataService, IHapticService haptic)
    {
        _dataService = dataService;
        _haptic = haptic;
        Title = "Recipes";

        SelectCategoryCommand = new Command<FoodCategory>((cat) =>
        {
            _haptic.PerformClick();
            SelectedCategory = (SelectedCategory == cat) ? null : cat;
        });

        ClearCategoryCommand = new Command(() =>
        {
            _haptic.PerformClick();
            SelectedCategory = null;
        });

        NavigateToRecipeCommand = new Command<Recipe>(async (recipe) =>
        {
            _haptic.PerformClick();
            if (recipe == null) return;

            if (IsSelectionMode)
            {
                // Ask which meal type, then add to plan for the selected day
                var mealType = await Shell.Current.DisplayActionSheet(
                    $"Add to {MealPlanDay}", "Cancel", null,
                    "Breakfast", "Lunch", "Dinner");

                if (mealType != null && mealType != "Cancel" && Enum.TryParse<MealType>(mealType, out var mt))
                {
                    _dataService.AddToMealPlan(new MealPlanEntry
                    {
                        Day = SelectedDay,
                        MealType = mt,
                        Recipe = recipe
                    });
                    await Shell.Current.DisplayAlert("Added",
                        $"{recipe.Name} added to {MealPlanDay} {mealType}.", "OK");
                    await Shell.Current.GoToAsync("//mealplan");
                }
            }
            else
            {
                await Shell.Current.GoToAsync($"recipedetail?id={recipe.Id}");
            }
        });

        LoadCategories();
        LoadAllRecipes();
    }

    private void LoadCategories()
    {
        Categories.Clear();
        foreach (FoodCategory cat in Enum.GetValues(typeof(FoodCategory)))
            Categories.Add(cat);
    }

    public void Reload() => FilterRecipes();

    private void LoadAllRecipes() => FilterRecipes();

    /// <summary>Applies current search text and category filter to the displayed recipe list.</summary>
    private void FilterRecipes()
    {
        Recipes.Clear();
        List<Recipe> results;

        if (!string.IsNullOrWhiteSpace(SearchText))
            results = _dataService.SearchRecipes(SearchText);
        else if (SelectedCategory.HasValue)
            results = _dataService.GetRecipesByCategory(SelectedCategory.Value);
        else
            results = _dataService.GetAllRecipes();

        foreach (var recipe in results)
            Recipes.Add(recipe);
    }
}
