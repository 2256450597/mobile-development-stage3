using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Handles quick recipe creation from a captured or picked photo.
/// Receives the photo path via the "photo" query parameter and allows the user
/// to enter a name, category, and optional nutritional details before saving.
/// When "editId" is provided, loads existing recipe data for editing.</summary>
[QueryProperty(nameof(PhotoPath), "photo")]
[QueryProperty(nameof(EditId), "editId")]
public class QuickAddViewModel : BaseViewModel
{
    private readonly IRecipeService _recipes;
    private readonly IHapticService _haptic;

    /// <summary>File path of the photo captured or picked on the Camera page.</summary>
    private string _photoPath = string.Empty;
    public string PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    /// <summary>Existing recipe ID when editing; empty for new recipes.</summary>
    private string _editId = string.Empty;
    public string EditId
    {
        get => _editId;
        set
        {
            if (SetProperty(ref _editId, value))
                _ = LoadExistingRecipe(value);
        }
    }

    private bool _isEditing;
    /// <summary>Whether the form is in edit mode (updating an existing recipe).</summary>
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    /// <summary>User-entered recipe name (required before saving).</summary>
    private string _recipeName = string.Empty;
    public string RecipeName
    {
        get => _recipeName;
        set => SetProperty(ref _recipeName, value);
    }

    /// <summary>Food category selected from the picker; defaults to Snack.</summary>
    private FoodCategory _selectedCategory = FoodCategory.Stovetop;
    public FoodCategory SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    /// <summary>Whether the optional details section is expanded.</summary>
    private bool _showDetails;
    public bool ShowDetails
    {
        get => _showDetails;
        set => SetProperty(ref _showDetails, value);
    }

    /// <summary>User-entered calorie count as a text string (parsed to int on save).</summary>
    private string _caloriesText = string.Empty;
    public string CaloriesText
    {
        get => _caloriesText;
        set => SetProperty(ref _caloriesText, value);
    }

    /// <summary>User-entered preparation time in minutes as a text string.</summary>
    private string _prepTimeText = string.Empty;
    public string PrepTimeText
    {
        get => _prepTimeText;
        set => SetProperty(ref _prepTimeText, value);
    }

    /// <summary>User-entered number of servings as a text string.</summary>
    private string _servingsText = string.Empty;
    public string ServingsText
    {
        get => _servingsText;
        set => SetProperty(ref _servingsText, value);
    }

    /// <summary>Difficulty level selected from the picker; defaults to "Easy".</summary>
    private string _selectedDifficulty = "Easy";
    public string SelectedDifficulty
    {
        get => _selectedDifficulty;
        set => SetProperty(ref _selectedDifficulty, value);
    }

    /// <summary>Label text for the details expander — changes based on expand state.</summary>
    private string _detailsSummary = "Add details: calories, prep time, servings...";
    public string DetailsSummary
    {
        get => _detailsSummary;
        set => SetProperty(ref _detailsSummary, value);
    }

    /// <summary>Available food categories for the category picker.</summary>
    public List<FoodCategory> CategoryOptions { get; }

    /// <summary>Difficulty levels offered in the picker.</summary>
    public List<string> DifficultyOptions { get; } = new() { "Easy", "Medium", "Hard" };

    /// <summary>Validates and saves the new recipe to the data store.</summary>
    public ICommand SaveCommand { get; }

    /// <summary>Discards the current form and navigates back.</summary>
    public ICommand DiscardCommand { get; }

    /// <summary>Expands or collapses the optional details section.</summary>
    public ICommand ToggleDetailsCommand { get; }

    /// <summary>Initialises the ViewModel with data and haptic services.
    /// Populates the category options from the FoodCategory enum.</summary>
    public QuickAddViewModel(IRecipeService recipes, IHapticService haptic)
    {
        _recipes = recipes;
        _haptic = haptic;
        Title = "Quick Add";

        CategoryOptions = Enum.GetValues<FoodCategory>().ToList();

        SaveCommand = new Command(async () => await OnSave());
        DiscardCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("..");
        });

        ToggleDetailsCommand = new Command(() =>
        {
            _haptic.PerformClick();
            ShowDetails = !ShowDetails;
            DetailsSummary = ShowDetails
                ? "Fill in below or leave blank to skip"
                : "Add details: calories, prep time, servings...";
        });
    }

    /// <summary>Loads an existing recipe's data into the form fields when editing.</summary>
    private async Task LoadExistingRecipe(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        // Small delay to ensure all properties are bound before loading
        await Task.Delay(50);
        var existing = _recipes.GetRecipeById(id);
        if (existing == null) return;

        IsEditing = true;
        Title = "Edit Recipe";
        RecipeName = existing.Name;
        SelectedCategory = existing.Category;
        PhotoPath = existing.ImageUrl;
        CaloriesText = existing.Calories > 0 ? existing.Calories.ToString() : string.Empty;
        PrepTimeText = existing.PrepTimeMinutes > 0 ? existing.PrepTimeMinutes.ToString() : string.Empty;
        ServingsText = existing.Servings > 0 ? existing.Servings.ToString() : string.Empty;
        SelectedDifficulty = existing.Difficulty;
    }

    /// <summary>Validates required fields, builds a Recipe object, persists it via
    /// the data service, and navigates back. Shows an error alert if saving fails.
    /// In edit mode, updates the existing recipe instead of creating a new one.</summary>
    private async Task OnSave()
    {
        if (string.IsNullOrWhiteSpace(RecipeName))
        {
            await Shell.Current.DisplayAlert("Missing Name", "Please enter a recipe name.", "OK");
            return;
        }

        if (RecipeName.Trim().Length > 50)
        {
            await Shell.Current.DisplayAlert("Name Too Long", "Recipe name must be 50 characters or fewer.", "OK");
            return;
        }

        _haptic.PerformLongPress();

        try
        {
            // Validate numeric fields: parse and check for negative values
            if (!string.IsNullOrWhiteSpace(CaloriesText) && (!int.TryParse(CaloriesText, out var cal) || cal < 0))
            {
                await Shell.Current.DisplayAlert("Invalid Calories", "Please enter a valid non-negative number for calories.", "OK");
                return;
            }
            int.TryParse(CaloriesText, out var calories);
            if (calories < 0) calories = 0;

            if (!string.IsNullOrWhiteSpace(PrepTimeText) && (!int.TryParse(PrepTimeText, out var pt) || pt < 0))
            {
                await Shell.Current.DisplayAlert("Invalid Prep Time", "Please enter a valid non-negative number for prep time.", "OK");
                return;
            }
            int.TryParse(PrepTimeText, out var prepTime);
            if (prepTime < 0) prepTime = 0;

            if (!string.IsNullOrWhiteSpace(ServingsText) && (!int.TryParse(ServingsText, out var sv) || sv < 0))
            {
                await Shell.Current.DisplayAlert("Invalid Servings", "Please enter a valid non-negative number for servings.", "OK");
                return;
            }
            int.TryParse(ServingsText, out var servings);
            if (servings < 0) servings = 0;

            if (IsEditing)
            {
                // Update existing recipe
                var existing = _recipes.GetRecipeById(EditId);
                if (existing != null)
                {
                    existing.Name = RecipeName.Trim();
                    existing.Category = SelectedCategory;
                    existing.ImageUrl = PhotoPath;
                    existing.Calories = calories;
                    existing.PrepTimeMinutes = prepTime;
                    existing.Servings = servings;
                    existing.Difficulty = SelectedDifficulty;
                    _recipes.UpdateRecipe(existing);
                }
                await Shell.Current.DisplayAlert("Updated", $"{RecipeName.Trim()} has been updated.", "OK");
            }
            else
            {
                var recipe = new Recipe
                {
                    Name = RecipeName.Trim(),
                    Category = SelectedCategory,
                    ImageUrl = PhotoPath,
                    Description = "Quickly added from camera.",
                    Ingredients = new List<string> { "Tap Edit to add ingredients" },
                    Instructions = new List<string> { "Tap Edit to add instructions" },
                    Calories = calories,
                    PrepTimeMinutes = prepTime,
                    Servings = servings,
                    Difficulty = SelectedDifficulty
                };

                _recipes.AddRecipe(recipe);
                await Shell.Current.DisplayAlert("Saved", $"{recipe.Name} has been added to your recipes.", "OK");
            }
            await Shell.Current.GoToAsync("../..");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Save Failed",
                "Unable to save the recipe. Please try again.", "OK");
        }
    }
}
