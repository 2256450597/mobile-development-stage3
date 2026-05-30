using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that lists all available recipes. Supports searching, filtering,
/// and selecting recipes to add to the meal plan.
/// </summary>
public partial class RecipesPage : ContentPage
{
    private readonly RecipesViewModel _viewModel;
    private readonly ThemeService _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipesPage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that manages the recipe list and selection state.</param>
    /// <param name="theme">Service for applying theme and font scaling.</param>
    public RecipesPage(RecipesViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// Called when the page appears. Applies font scaling, enters or exits
    /// selection mode based on a pending day selection, and reloads the recipe list.
    /// </summary>
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

    /// <summary>
    /// Called when the page disappears. Exits selection mode to clean up state.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ExitSelectionMode();
    }
}
