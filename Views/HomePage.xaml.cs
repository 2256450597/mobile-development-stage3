using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private readonly ThemeService _theme;

    public HomePage(HomeViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _theme = theme;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Reload();
        _theme.ApplyFontScaleToPage(this);
    }

    private async void OnLogoTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//home");
    }
}
