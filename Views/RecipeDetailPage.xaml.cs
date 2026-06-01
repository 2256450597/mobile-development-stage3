using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that displays the full details of a single recipe, including
/// ingredients, instructions, and nutritional information.
/// </summary>
public partial class RecipeDetailPage : ContentPage
{
    private readonly ThemeService _theme;

    public RecipeDetailPage(RecipeDetailViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = viewModel;
    }
}
