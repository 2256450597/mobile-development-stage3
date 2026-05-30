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

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeDetailPage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that provides recipe detail data.</param>
    /// <param name="theme">Service for applying theme and font scaling.</param>
    public RecipeDetailPage(RecipeDetailViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = viewModel;
    }
}
