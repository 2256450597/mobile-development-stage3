using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

[QueryProperty(nameof(PhotoPath), "photo")]
public class QuickAddViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IHapticService _haptic;

    private string _photoPath = string.Empty;
    public string PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    private string _recipeName = string.Empty;
    public string RecipeName
    {
        get => _recipeName;
        set => SetProperty(ref _recipeName, value);
    }

    private FoodCategory _selectedCategory = FoodCategory.Snack;
    public FoodCategory SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    // Optional details (tappable to reveal)
    private bool _showDetails;
    public bool ShowDetails
    {
        get => _showDetails;
        set => SetProperty(ref _showDetails, value);
    }

    private string _caloriesText = string.Empty;
    public string CaloriesText
    {
        get => _caloriesText;
        set => SetProperty(ref _caloriesText, value);
    }

    private string _prepTimeText = string.Empty;
    public string PrepTimeText
    {
        get => _prepTimeText;
        set => SetProperty(ref _prepTimeText, value);
    }

    private string _servingsText = string.Empty;
    public string ServingsText
    {
        get => _servingsText;
        set => SetProperty(ref _servingsText, value);
    }

    private string _selectedDifficulty = "Easy";
    public string SelectedDifficulty
    {
        get => _selectedDifficulty;
        set => SetProperty(ref _selectedDifficulty, value);
    }

    private string _detailsSummary = "Add details: calories, prep time, servings...";
    public string DetailsSummary
    {
        get => _detailsSummary;
        set => SetProperty(ref _detailsSummary, value);
    }

    public List<FoodCategory> CategoryOptions { get; }
    public List<string> DifficultyOptions { get; } = new() { "Easy", "Medium", "Hard" };

    public ICommand SaveCommand { get; }
    public ICommand DiscardCommand { get; }
    public ICommand ToggleDetailsCommand { get; }

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

    private async Task OnSave()
    {
        if (string.IsNullOrWhiteSpace(RecipeName))
        {
            await Shell.Current.DisplayAlert("Missing Name", "Please enter a recipe name.", "OK");
            return;
        }

        _haptic.PerformLongPress();

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
}
