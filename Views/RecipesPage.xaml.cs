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

        if (RecipesViewModel.PendingSelectionDay.HasValue)
        {
            _viewModel.EnterSelectionMode(RecipesViewModel.PendingSelectionDay.Value);
            RecipesViewModel.PendingSelectionDay = null;
        }
        else
        {
            _viewModel.ExitSelectionMode();
        }

        _viewModel.Reload();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ExitSelectionMode();
    }
}
