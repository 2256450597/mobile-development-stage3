using System.Collections.ObjectModel;
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

    public List<FoodCategory> CategoryOptions { get; }

    public ICommand SaveCommand { get; }
    public ICommand DiscardCommand { get; }

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
    }

    private async Task OnSave()
    {
        if (string.IsNullOrWhiteSpace(RecipeName))
        {
            await Shell.Current.DisplayAlert("Missing Name", "Please enter a recipe name.", "OK");
            return;
        }

        _haptic.PerformLongPress();

        var recipe = new Recipe
        {
            Name = RecipeName.Trim(),
            Category = SelectedCategory,
            ImageUrl = PhotoPath,
            Description = "Added from camera capture.",
            Ingredients = new List<string> { "Add ingredients later" },
            Instructions = new List<string> { "Add instructions later" },
            PrepTimeMinutes = 10,
            CookTimeMinutes = 10,
            Calories = 300,
            Servings = 2,
            Difficulty = "Easy"
        };

        _dataService.AddRecipe(recipe);
        await Shell.Current.DisplayAlert("Saved", $"{recipe.Name} has been added to your recipes.", "OK");
        await Shell.Current.GoToAsync("../..");
    }
}
