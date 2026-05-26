using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

[QueryProperty(nameof(RecipeId), "id")]
public class RecipeDetailViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    private string _recipeId = string.Empty;
    public string RecipeId
    {
        get => _recipeId;
        set
        {
            SetProperty(ref _recipeId, value);
            LoadRecipe(value);
        }
    }

    private Recipe? _recipe;
    public Recipe? Recipe
    {
        get => _recipe;
        set => SetProperty(ref _recipe, value);
    }

    public ObservableCollection<string> Ingredients { get; } = new();
    public ObservableCollection<string> Instructions { get; } = new();

    public ICommand AddToMealPlanCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand SpeakRecipeCommand { get; }
    public ICommand GoBackCommand { get; }

    public RecipeDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;

        AddToMealPlanCommand = new Command(OnAddToMealPlan);
        ToggleFavoriteCommand = new Command(OnToggleFavorite);
        SpeakRecipeCommand = new Command(async () => await OnSpeakRecipe());
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private void LoadRecipe(string id)
    {
        var recipe = _dataService.GetRecipeById(id);
        if (recipe == null) return;

        Recipe = recipe;
        Title = recipe.Name;

        Ingredients.Clear();
        foreach (var ing in recipe.Ingredients)
            Ingredients.Add(ing);

        Instructions.Clear();
        for (int i = 0; i < recipe.Instructions.Count; i++)
            Instructions.Add($"{i + 1}. {recipe.Instructions[i]}");
    }

    private async void OnAddToMealPlan()
    {
        if (Recipe == null) return;

        var day = DateTime.Now.DayOfWeek;
        var entry = new MealPlanEntry
        {
            Day = day,
            MealType = MealType.Dinner,
            Recipe = Recipe
        };
        _dataService.AddToMealPlan(entry);

        await Shell.Current.DisplayAlert("Added", $"{Recipe.Name} added to today's meal plan.", "OK");
    }

    private void OnToggleFavorite()
    {
        if (Recipe != null)
        {
            Recipe.IsFavorite = !Recipe.IsFavorite;
            OnPropertyChanged(nameof(Recipe));
        }
    }

    private async Task OnSpeakRecipe()
    {
        if (Recipe == null) return;
        await TextToSpeech.Default.SpeakAsync(
            $"{Recipe.Name}. Ingredients: {string.Join(", ", Recipe.Ingredients)}. " +
            $"Instructions: {string.Join(". ", Recipe.Instructions)}");
    }
}
