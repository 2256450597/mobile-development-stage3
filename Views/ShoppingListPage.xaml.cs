using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that displays the user's shopping list, automatically generated
/// from the current meal plan or manually managed by the user.
/// </summary>
public partial class ShoppingListPage : ContentPage
{
    private readonly ShoppingListViewModel _viewModel;

    private readonly ThemeService _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingListPage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that manages shopping list items.</param>
    /// <param name="theme">Service for applying theme and font scaling.</param>
    public ShoppingListPage(ShoppingListViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// Called when the page appears. Applies font scaling and loads
    /// the shopping list items from the view model.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _theme.ApplyFontScaleToPage(this);
        _viewModel.LoadItems();
    }
}
