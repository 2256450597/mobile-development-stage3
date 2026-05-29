using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class ShoppingListPage : ContentPage
{
    private readonly ShoppingListViewModel _viewModel;

    private readonly ThemeService _theme;

    public ShoppingListPage(ShoppingListViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _theme.ApplyFontScaleToPage(this);
        _viewModel.LoadItems();
    }
}
