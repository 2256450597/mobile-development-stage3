using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

[QueryProperty(nameof(RecipeId), "id")]
/// <summary>Displays a single recipe with ingredients, instructions, and TTS/hardware actions.</summary>
public class RecipeDetailViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly ITextToSpeechService _tts;
    private readonly IHapticService _haptic;

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

    private bool _isSpeaking;
    public bool IsSpeaking
    {
        get => _isSpeaking;
        set => SetProperty(ref _isSpeaking, value);
    }

    // Uses pitch (0.5–2.0) — MAUI SpeechOptions does not support rate/speed natively.
    private float _ttsPitch = 1.1f;
    public float TtsPitch
    {
        get => _ttsPitch;
        set
        {
            if (SetProperty(ref _ttsPitch, value))
                _tts.Pitch = value;
        }
    }

    public ObservableCollection<string> Ingredients { get; } = new();
    public ObservableCollection<string> Instructions { get; } = new();

    public ICommand AddToMealPlanCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand SpeakRecipeCommand { get; }
    public ICommand StopSpeakingCommand { get; }
    public ICommand GoBackCommand { get; }

    public RecipeDetailViewModel(IDataService dataService,
                                  ITextToSpeechService tts,
                                  IHapticService haptic)
    {
        _dataService = dataService;
        _tts = tts;
        _haptic = haptic;

        AddToMealPlanCommand = new Command(OnAddToMealPlan);
        ToggleFavoriteCommand = new Command(OnToggleFavorite);
        SpeakRecipeCommand = new Command(async () => await OnSpeakRecipe());
        StopSpeakingCommand = new Command(async () => await OnStopSpeaking());
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    /// <summary>Fetches recipe by ID from the data service and populates all display collections.</summary>
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

    /// <summary>Adds the current recipe to today's dinner slot in the meal plan.</summary>
    private async void OnAddToMealPlan()
    {
        if (Recipe == null) return;
        _haptic.PerformLongPress();

        var day = DateTime.Now.DayOfWeek;
        var entry = new MealPlanEntry
        {
            Day = day,
            MealType = MealType.Dinner,
            Recipe = Recipe
        };
        _dataService.AddToMealPlan(entry);

        await Shell.Current.DisplayAlert("Added",
            $"{Recipe.Name} added to today's meal plan.", "OK");
    }

    private void OnToggleFavorite()
    {
        _haptic.PerformClick();
        if (Recipe != null)
        {
            Recipe.IsFavorite = !Recipe.IsFavorite;
            OnPropertyChanged(nameof(Recipe));
        }
    }

    /// <summary>Reads the full recipe aloud using the text-to-speech service with natural English narration.</summary>
    private async Task OnSpeakRecipe()
    {
        if (Recipe == null) return;
        _haptic.PerformClick();

        try
        {
            IsSpeaking = true;
            var ingredientList = string.Join(". ", Recipe.Ingredients.Select(ing => ing));
            var instructionSteps = string.Join(". Next, ", Recipe.Instructions);
            var text = $"Let's make {Recipe.Name}. " +
                       $"Here are the ingredients. {ingredientList}. " +
                       $"Now follow these steps. First, {instructionSteps}. Enjoy your meal!";

            await _tts.SpeakAsync(text, 1.0f, TtsPitch);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("TTS Error", ex.Message, "OK");
        }
        finally
        {
            IsSpeaking = false;
        }
    }

    private async Task OnStopSpeaking()
    {
        await _tts.StopAsync();
        IsSpeaking = false;
    }
}
