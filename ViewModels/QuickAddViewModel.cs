using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Handles quick recipe creation from a captured or picked photo.
/// Receives the photo path via the "photo" query parameter and allows the user
/// to enter a name, category, and optional nutritional details before saving.</summary>
[QueryProperty(nameof(PhotoPath), "photo")]
public class QuickAddViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IHapticService _haptic;

    /// <summary>File path of the photo captured or picked on the Camera page.</summary>
    private string _photoPath = string.Empty;
    public string PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    /// <summary>User-entered recipe name (required before saving).</summary>
    private string _recipeName = string.Empty;
    public string RecipeName
    {
        get => _recipeName;
        set => SetProperty(ref _recipeName, value);
    }

    /// <summary>Food category selected from the picker; defaults to Snack.</summary>
    private FoodCategory _selectedCategory = FoodCategory.Snack;
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
    public QuickAddViewModel(IDataService dataService, IHapticService haptic)
    {
        _dataService = dataService;
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

    /// <summary>Validates required fields, builds a Recipe object, persists it via
    /// the data service, and navigates back. Shows an error alert if saving fails.</summary>
    private async Task OnSave()
    {
        if (string.IsNullOrWhiteSpace(RecipeName))
        {
            await Shell.Current.DisplayAlert("Missing Name", "Please enter a recipe name.", "OK");
            return;
        }

        _haptic.PerformLongPress();

        try
        {
            int.TryParse(CaloriesText, out var calories);
            if (calories <= 0) calories = 0;

            int.TryParse(PrepTimeText, out var prepTime);
            if (prepTime <= 0) prepTime = 0;

            int.TryParse(ServingsText, out var servings);
            if (servings <= 0) servings = 0;

            var recipe = new Recipe
            {
                Name = RecipeName.Trim(),
                Category = SelectedCategory,
                ImageUrl = PhotoPath,
                Description = "Added from camera capture.",
                Ingredients = new List<string> { "Add ingredients later" },
                Instructions = new List<string> { "Add instructions later" },
                Calories = calories,
                PrepTimeMinutes = prepTime,
                Servings = servings,
                Difficulty = SelectedDifficulty
            };

            _dataService.AddRecipe(recipe);
            await Shell.Current.DisplayAlert("Saved", $"{recipe.Name} has been added to your recipes.", "OK");
            await Shell.Current.GoToAsync("../..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Save Failed",
                $"Could not save the recipe. {ex.Message}", "OK");
        }
    }
}
