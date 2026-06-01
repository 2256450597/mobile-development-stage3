using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Helpers;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Searchable/filterable recipe browser with category tabs and real-time filtering.</summary>
public class RecipesViewModel : BaseViewModel
{
    private readonly IRecipeService _recipes;
    private readonly IMealPlanService _mealPlan;
    private readonly IShoppingListService _shoppingList;
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

    /// <summary>Called when the page appears. Enters/exits selection mode based on pending state, then reloads.</summary>
    public void OnPageAppearing()
    {
        if (PendingSelectionDay.HasValue)
        {
            EnterSelectionMode(PendingSelectionDay.Value);
            PendingSelectionDay = null;
        }
        else
        {
            ExitSelectionMode();
        }
        Reload();
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
    /// <summary>Command to refresh the recipe list with a smooth animated delay.</summary>
    public ICommand RefreshCommand { get; }

    /// <summary>Initialises a new instance of the <see cref="RecipesViewModel"/> class with data and haptic services, loading categories and recipes.</summary>
    public RecipesViewModel(IRecipeService recipes, IMealPlanService mealPlan, IShoppingListService shoppingList, IHapticService haptic)
    {
        _recipes = recipes;
        _mealPlan = mealPlan;
        _shoppingList = shoppingList;
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
                    // Check for duplicate: same recipe, same day, same meal type
                    var monday = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + 1);
                    var alreadyPlanned = _mealPlan.GetMealPlanForWeek(monday)
                        .Any(e => e.Day == SelectedDay && e.MealType == mt && e.Recipe?.Id == recipe.Id);
                    if (alreadyPlanned)
                    {
                        await Shell.Current.DisplayAlert("Already Planned",
                            $"{recipe.Name} is already in {MealPlanDay} {mealType}.", "OK");
                        return;
                    }

                    _mealPlan.AddToMealPlan(new MealPlanEntry
                    {
                        Day = SelectedDay,
                        MealType = mt,
                        Recipe = recipe
                    });

                    // Auto-add ingredients to shopping list if meal is for today
                    if (SelectedDay == DateTime.Now.DayOfWeek)
                        RegenerateShoppingList();

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

        RefreshCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            FilterRecipes();
            await Task.Delay(400);
            IsBusy = false;
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

    /// <summary>Regenerates auto-generated shopping items from today's meal plan.</summary>
    private void RegenerateShoppingList()
    {
        _shoppingList.RemoveAutoGeneratedItems();

        var today = DateTime.Now.DayOfWeek;
        var monday = DateTime.Now.AddDays(-(int)today + 1);
        var todayMeals = _mealPlan.GetMealPlanForWeek(monday)
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

    /// <summary>Loads all recipes into the displayed list by applying the current filters.</summary>
    private void LoadAllRecipes() => FilterRecipes();

    /// <summary>Applies current search text and category filter to the displayed recipe list.</summary>
    private void FilterRecipes()
    {
        Recipes.Clear();
        List<Recipe> results;

        if (!string.IsNullOrWhiteSpace(SearchText))
            results = _recipes.SearchRecipes(SearchText);
        else if (SelectedCategory.HasValue)
            results = _recipes.GetRecipesByCategory(SelectedCategory.Value);
        else
            results = _recipes.GetAllRecipes();

        foreach (var recipe in results)
            Recipes.Add(recipe);
    }
}
