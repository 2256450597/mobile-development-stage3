using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class RecipesPage : ContentPage
{
    private readonly RecipesViewModel _viewModel;

    public RecipesPage(RecipesViewModel viewModel)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _theme.ApplyFontScaleToPage(this);

        // Check if navigated from MealPlan "Add" button
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
