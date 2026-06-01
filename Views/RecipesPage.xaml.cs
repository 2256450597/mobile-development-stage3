using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class RecipesPage : ContentPage
{
    private readonly RecipesViewModel _viewModel;
    private readonly ThemeService _theme;

    public RecipesPage(RecipesViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _theme.ApplyFontScaleToPage(this);
        _viewModel.OnPageAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ExitSelectionMode();
    }
}
