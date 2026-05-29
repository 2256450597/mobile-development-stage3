using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

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
