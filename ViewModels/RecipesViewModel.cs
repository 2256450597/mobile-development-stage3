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

    /// <summary>Gets the collection of filtered recipes currently displayed to the user.</summary>
    public ObservableCollection<Recipe> Recipes { get; } = new();
    /// <summary>Gets the collection of available food categories used for filtering.</summary>
    public ObservableCollection<FoodCategory> Categories { get; } = new();

    private string _searchText = string.Empty;
    /// <summary>Gets or sets the search text used to filter recipes in real time.</summary>
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
    /// <summary>Gets or sets the selected food category filter, triggering a re-filter on change.</summary>
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
    /// <summary>Gets or sets the day for which a recipe is being selected from the meal plan view.</summary>
    public static DayOfWeek? PendingSelectionDay;

    private string _mealPlanDay = string.Empty;
    /// <summary>Gets or sets the display string for the selected meal plan day.</summary>
    public string MealPlanDay
    {
        get => _mealPlanDay;
        set => SetProperty(ref _mealPlanDay, value);
    }

    /// <summary>Configures the view model for meal plan selection mode for the specified day of the week.</summary>
    public void EnterSelectionMode(DayOfWeek day)
    {
        var dayStr = day.ToString();
        MealPlanDay = dayStr;
        SelectedDay = day;
        IsSelectionMode = true;
        Title = $"Pick for {dayStr}";
    }

    /// <summary>Resets the view model from meal plan selection mode back to normal recipe browsing.</summary>
    public void ExitSelectionMode()
    {
        IsSelectionMode = false;
        MealPlanDay = string.Empty;
        Title = "Recipes";
    }

    private bool _isSelectionMode;
    /// <summary>Gets or sets whether the view is in meal plan selection mode.</summary>
    public bool IsSelectionMode
    {
        get => _isSelectionMode;
        set => SetProperty(ref _isSelectionMode, value);
    }

    private DayOfWeek _selectedDay;
    /// <summary>Gets or sets the selected day of the week for meal plan assignment.</summary>
    public DayOfWeek SelectedDay
    {
        get => _selectedDay;
        set => SetProperty(ref _selectedDay, value);
    }

    /// <summary>Command to select or toggle a food category as a filter.</summary>
    public ICommand SelectCategoryCommand { get; }
    /// <summary>Command to clear the current category filter and show all recipes.</summary>
    public ICommand ClearCategoryCommand { get; }
    /// <summary>Command to navigate to a recipe detail page or add it to the meal plan in selection mode.</summary>
    public ICommand NavigateToRecipeCommand { get; }

    /// <summary>Initialises a new instance of the <see cref="RecipesViewModel"/> class with data and haptic services, loading categories and recipes.</summary>
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

    /// <summary>Populates the Categories collection with all values from the FoodCategory enum.</summary>
    private void LoadCategories()
    {
        Categories.Clear();
        foreach (FoodCategory cat in Enum.GetValues(typeof(FoodCategory)))
            Categories.Add(cat);
    }

    /// <summary>Triggers a re-filter of the recipe list. Called when returning to the page.</summary>
    public void Reload() => FilterRecipes();

    /// <summary>Loads all recipes into the displayed list by applying the current filters.</summary>
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
