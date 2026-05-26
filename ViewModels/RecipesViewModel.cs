using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class RecipesViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

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

    public ICommand SelectCategoryCommand { get; }
    public ICommand ClearCategoryCommand { get; }
    public ICommand NavigateToRecipeCommand { get; }

    public RecipesViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Recipes";

        SelectCategoryCommand = new Command<FoodCategory>((cat) =>
        {
            if (SelectedCategory == cat)
                SelectedCategory = null;
            else
                SelectedCategory = cat;
        });

        ClearCategoryCommand = new Command(() => SelectedCategory = null);

        NavigateToRecipeCommand = new Command<Recipe>(async (recipe) =>
        {
            if (recipe != null)
                await Shell.Current.GoToAsync($"recipedetail?id={recipe.Id}");
        });

        LoadCategories();
        LoadAllRecipes();
    }

    private void LoadCategories()
    {
        Categories.Clear();
        foreach (FoodCategory cat in Enum.GetValues(typeof(FoodCategory)))
        {
            Categories.Add(cat);
        }
    }

    private void LoadAllRecipes()
    {
        FilterRecipes();
    }

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
